@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Name of the Container App')
param containerAppName string = 'app-supportagentui'

@description('Existing shared Container App Environment name')
param containerAppEnvName string = 'cae-h2-experiments-dev'

@description('Existing Azure Container Registry name (without .azurecr.io)')
param acrName string = 'h2experimentsacr'

@description('Container image tag to deploy')
param imageTag string = 'latest'

@description('Azure AI Foundry resource group')
param foundryRg string = 'rg-h2-foundry'

@description('Azure AI Foundry CognitiveServices account name')
param foundryAccountName string = 'H2-Experiments'

// ── References to existing shared resources ──────────────────────────────────
// Both the Container App Environment and ACR are in the same RG as this deployment

resource sharedEnv 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerAppEnvName
}

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: acrName
}

// ── User-assigned managed identity (created first so RBAC can be pre-assigned)

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-${containerAppName}'
  location: location
}

// ── RBAC: AcrPull — must exist BEFORE the Container App starts pulling ────────

var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'

resource acrPullAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, identity.id, acrPullRoleId)
  scope: acr
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// ── Container App — uses pre-provisioned identity with AcrPull ───────────────

module containerApp 'modules/containerApp.bicep' = {
  name: 'containerApp'
  dependsOn: [acrPullAssignment]   // wait for AcrPull to propagate
  params: {
    location: location
    containerAppName: containerAppName
    containerAppEnvId: sharedEnv.id
    acrLoginServer: acr.properties.loginServer
    imageTag: imageTag
    identityResourceId: identity.id
    identityClientId: identity.properties.clientId
  }
}

// ── RBAC: Azure AI Developer on Foundry project ───────────────────────────────

module foundryRbac 'modules/foundryRbac.bicep' = {
  name: 'foundryRbac'
  scope: resourceGroup(foundryRg)
  params: {
    foundryAccountName: foundryAccountName
    principalId: identity.properties.principalId
  }
}

output containerAppUrl string = containerApp.outputs.fqdn


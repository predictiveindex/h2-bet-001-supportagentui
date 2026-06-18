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

@description('Storage account name for vote data')
param storageAccountName string = 'sth2validatedev'

// ── References to existing shared resources ──────────────────────────────────

resource sharedEnv 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerAppEnvName
}

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: acrName
}

// ── User-assigned managed identity ───────────────────────────────────────────

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-${containerAppName}'
  location: location
}

// ── RBAC: AcrPull ─────────────────────────────────────────────────────────────

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

// ── Storage account for vote data ─────────────────────────────────────────────

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-05-01' = {
  name: 'default'
  parent: storageAccount
}

resource bet001Table 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-05-01' = {
  name: 'bet001'
  parent: tableService
}

// Storage Table Data Contributor
var storageTableDataContributorRoleId = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'

resource storageTableRbac 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, identity.id, storageTableDataContributorRoleId)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageTableDataContributorRoleId)
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// ── Container App ─────────────────────────────────────────────────────────────

module containerApp 'modules/containerApp.bicep' = {
  name: 'containerApp'
  dependsOn: [acrPullAssignment, storageTableRbac]
  params: {
    location: location
    containerAppName: containerAppName
    containerAppEnvId: sharedEnv.id
    acrLoginServer: acr.properties.loginServer
    imageTag: imageTag
    identityResourceId: identity.id
    identityClientId: identity.properties.clientId
    storageAccountUrl: 'https://${storageAccount.name}.table.core.windows.net'
  }
}

// ── RBAC: Azure AI Developer on Foundry ───────────────────────────────────────

module foundryRbac 'modules/foundryRbac.bicep' = {
  name: 'foundryRbac'
  scope: resourceGroup(foundryRg)
  params: {
    foundryAccountName: foundryAccountName
    principalId: identity.properties.principalId
  }
}

output containerAppUrl string = containerApp.outputs.fqdn

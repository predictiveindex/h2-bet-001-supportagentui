@description('Azure region for all resources')
param location string = resourceGroup().location

@description('Name of the Container App')
param containerAppName string = 'app-supportagentui'

@description('Existing shared Container App Environment name')
param containerAppEnvName string = 'cae-h2-experiments-dev'

@description('Resource group containing the shared Container App Environment and ACR')
param sharedRg string = 'rg-h2-experiments-dev'

@description('Existing Azure Container Registry name (without .azurecr.io)')
param acrName string = 'h2experimentsacr'

@description('Container image tag to deploy')
param imageTag string = 'latest'

@description('Azure AI Foundry resource group')
param foundryRg string = 'rg_h2_foundry'

@description('Azure AI Foundry workspace / project name')
param foundryWorkspaceName string = 'H2-BET-001'

// ── References to existing shared resources ──────────────────────────────────

resource sharedEnv 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerAppEnvName
  scope: resourceGroup(sharedRg)
}

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: acrName
  scope: resourceGroup(sharedRg)
}

// ── Container App ─────────────────────────────────────────────────────────────

module containerApp 'modules/containerApp.bicep' = {
  name: 'containerApp'
  params: {
    location: location
    containerAppName: containerAppName
    containerAppEnvId: sharedEnv.id
    acrLoginServer: acr.properties.loginServer
    imageTag: imageTag
    acrName: acrName
    foundryRg: foundryRg
    foundryWorkspaceName: foundryWorkspaceName
  }
}

output containerAppUrl string = containerApp.outputs.fqdn

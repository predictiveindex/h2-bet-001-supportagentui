@description('Azure region')
param location string

@description('Name of the Container App')
param containerAppName string

@description('Resource ID of the Container App Environment')
param containerAppEnvId string

@description('ACR login server (e.g. h2experimentsacr.azurecr.io)')
param acrLoginServer string

@description('Image tag to deploy')
param imageTag string

@description('ACR name (for existing resource reference)')
param acrName string

@description('Azure AI Foundry resource group')
param foundryRg string

@description('Azure AI Foundry workspace / project name')
param foundryWorkspaceName string

// ── Container App ─────────────────────────────────────────────────────────────

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    environmentId: containerAppEnvId
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
      registries: [
        {
          server: acrLoginServer
          identity: 'system'
        }
      ]
    }
    template: {
      containers: [
        {
          name: containerAppName
          image: '${acrLoginServer}/supportagentui:${imageTag}'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 3
      }
    }
  }
}

// ── RBAC: AcrPull on shared ACR ───────────────────────────────────────────────

var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'

resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: acrName
}

resource acrPullAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, containerApp.id, acrPullRoleId)
  scope: acr
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
    principalId: containerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// ── RBAC: Azure AI Developer on Foundry project ───────────────────────────────

var aiDeveloperRoleId = '64702f94-c441-49e6-a78b-ef80e0188fee'

resource foundryWorkspace 'Microsoft.MachineLearningServices/workspaces@2024-04-01' existing = {
  name: foundryWorkspaceName
  scope: resourceGroup(foundryRg)
}

resource aiDeveloperAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(foundryWorkspace.id, containerApp.id, aiDeveloperRoleId)
  scope: foundryWorkspace
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', aiDeveloperRoleId)
    principalId: containerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

output fqdn string = 'https://${containerApp.properties.configuration.ingress.fqdn}'


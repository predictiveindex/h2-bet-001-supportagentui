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

@description('Resource ID of the user-assigned managed identity')
param identityResourceId string

// ── Container App ─────────────────────────────────────────────────────────────

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identityResourceId}': {}
    }
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
          identity: identityResourceId
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

output fqdn string = 'https://${containerApp.properties.configuration.ingress.fqdn}'



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

@description('Client ID of the user-assigned managed identity (passed as AZURE_CLIENT_ID env var)')
param identityClientId string

@description('Azure Table Storage account URL (e.g. https://sth2validatedev.table.core.windows.net)')
param storageAccountUrl string

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
          env: [
            {
              name: 'AZURE_CLIENT_ID'
              value: identityClientId
            }
            {
              name: 'AZURE_STORAGE_ACCOUNT_URL'
              value: storageAccountUrl
            }
          ]
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

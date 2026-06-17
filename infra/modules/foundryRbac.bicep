@description('Azure AI Services account name (AI Foundry hub)')
param foundryAccountName string = 'H2-Experiments'

@description('Principal ID of the Container App managed identity')
param principalId string

var aiDeveloperRoleId = '64702f94-c441-49e6-a78b-ef80e0188fee'

resource foundryAccount 'Microsoft.CognitiveServices/accounts@2024-10-01' existing = {
  name: foundryAccountName
}

resource aiDeveloperAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(foundryAccount.id, principalId, aiDeveloperRoleId)
  scope: foundryAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', aiDeveloperRoleId)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

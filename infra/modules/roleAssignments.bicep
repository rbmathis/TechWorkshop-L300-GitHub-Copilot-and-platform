// Role Assignments module
// Assigns the AcrPull role to the App Service managed identity for container image access

@description('The principal ID of the App Service managed identity')
param appServicePrincipalId string

@description('The name of the Azure Container Registry')
param acrName string

// Reference to existing ACR
resource acr 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' existing = {
  name: acrName
}

// Built-in Azure role definition ID for AcrPull
// See: https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#acrpull
var acrPullRoleDefinitionId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

// Assign AcrPull role to App Service managed identity
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, appServicePrincipalId, acrPullRoleDefinitionId)
  scope: acr
  properties: {
    roleDefinitionId: acrPullRoleDefinitionId
    principalId: appServicePrincipalId
    principalType: 'ServicePrincipal'
  }
}

@description('The resource ID of the role assignment')
output roleAssignmentId string = roleAssignment.id

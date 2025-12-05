// Azure Container Registry module
// Provides private container image storage with RBAC-only access

@description('The name of the container registry (alphanumeric only)')
param name string

@description('The Azure region for the registry')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('The SKU of the container registry')
@allowed([
  'Basic'
  'Standard'
  'Premium'
])
param sku string = 'Basic'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: sku
  }
  properties: {
    adminUserEnabled: false // RBAC only, no admin credentials
    anonymousPullEnabled: false // Disable anonymous pull access for security
    dataEndpointEnabled: false
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
    zoneRedundancy: sku == 'Premium' ? 'Enabled' : 'Disabled'
    policies: {
      quarantinePolicy: {
        status: 'disabled'
      }
      trustPolicy: {
        status: 'disabled'
        type: 'Notary'
      }
      retentionPolicy: {
        status: 'disabled'
        days: 7
      }
      exportPolicy: {
        status: 'enabled'
      }
      azureADAuthenticationAsArmPolicy: {
        status: 'enabled'
      }
      softDeletePolicy: {
        status: 'disabled'
        retentionDays: 7
      }
    }
    encryption: {
      status: 'disabled'
    }
  }
}

@description('The resource ID of the container registry')
output id string = containerRegistry.id

@description('The name of the container registry')
output name string = containerRegistry.name

@description('The login server URL of the container registry')
output loginServer string = containerRegistry.properties.loginServer

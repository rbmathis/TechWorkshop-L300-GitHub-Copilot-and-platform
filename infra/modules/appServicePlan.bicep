// App Service Plan module
// Provides compute resources for App Service with Linux container support

@description('The name of the App Service Plan')
param name string

@description('The Azure region for the plan')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('The SKU for the App Service Plan')
param sku object = {
  name: 'B1'
  tier: 'Basic'
  size: 'B1'
  family: 'B'
  capacity: 1
}

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: name
  location: location
  tags: tags
  sku: sku
  kind: 'linux'
  properties: {
    reserved: true // Required for Linux
    zoneRedundant: false
    targetWorkerCount: 1
    targetWorkerSizeId: 0
  }
}

@description('The resource ID of the App Service Plan')
output id string = appServicePlan.id

@description('The name of the App Service Plan')
output name string = appServicePlan.name

// Azure AI Foundry module
// Provides AI model hosting and management capabilities

@description('The name of the AI Foundry workspace')
param name string

@description('The Azure region for the AI Foundry workspace')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('The friendly name for the AI Foundry workspace')
param friendlyName string = 'Zava Storefront AI Workspace'

@description('The description for the AI Foundry workspace')
param workspaceDescription string = 'AI Foundry workspace for Zava Storefront AI capabilities'

// Storage account for AI Foundry
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: 'st${uniqueString(resourceGroup().id, name)}'
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true // Required for AI Foundry
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
  }
}

// Key Vault for AI Foundry
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'kv-${uniqueString(resourceGroup().id, name)}'
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enableRbacAuthorization: true
    enablePurgeProtection: true
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
  }
}

// Application Insights for AI Foundry
resource aiInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-${name}'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Bluefield'
    Request_Source: 'rest'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// AI Foundry workspace (Machine Learning workspace)
resource aiFoundry 'Microsoft.MachineLearningServices/workspaces@2024-04-01' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    friendlyName: friendlyName
    description: workspaceDescription
    storageAccount: storageAccount.id
    keyVault: keyVault.id
    applicationInsights: aiInsights.id
    publicNetworkAccess: 'Enabled'
    v1LegacyMode: false
  }
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
}

@description('The resource ID of the AI Foundry workspace')
output id string = aiFoundry.id

@description('The name of the AI Foundry workspace')
output name string = aiFoundry.name

@description('The principal ID of the AI Foundry managed identity')
output principalId string = aiFoundry.identity.principalId

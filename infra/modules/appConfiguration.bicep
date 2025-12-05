// Azure App Configuration resource
// Used for centralized configuration and feature flag management

param location string
param appConfigName string
param tags object = {}

@description('The SKU of the App Configuration store')
param sku string = 'standard'

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: appConfigName
  location: location
  sku: {
    name: sku
  }
  tags: tags
  properties: {
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
  }
}

// Create some default feature flags
resource newCheckoutFlag 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfig
  name: '.appconfig.featureflag~2FNewCheckout'
  properties: {
    value: jsonString({
      id: 'NewCheckout'
      description: 'Enables the new checkout experience'
      enabled: true
      conditions: {
        clientFilters: []
      }
    })
    contentType: 'application/vnd.microsoft.appconfig.ff+json;charset=utf-8'
  }
}

resource recommendationEngineFlag 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfig
  name: '.appconfig.featureflag~2FRecommendationEngine'
  properties: {
    value: jsonString({
      id: 'RecommendationEngine'
      description: 'Enables the recommendation engine feature'
      enabled: false
      conditions: {
        clientFilters: []
      }
    })
    contentType: 'application/vnd.microsoft.appconfig.ff+json;charset=utf-8'
  }
}

resource bulkDiscountsFlag 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfig
  name: '.appconfig.featureflag~2FBulkDiscounts'
  properties: {
    value: jsonString({
      id: 'BulkDiscounts'
      description: 'Enables bulk discount functionality'
      enabled: true
      conditions: {
        clientFilters: []
      }
    })
    contentType: 'application/vnd.microsoft.appconfig.ff+json;charset=utf-8'
  }
}

// Output the connection string
output connectionString string = appConfig.listKeys().value[0].connectionString
output appConfigName string = appConfig.name
output appConfigId string = appConfig.id

// Helper function to convert object to JSON string
func jsonString(obj object) string => string(obj)

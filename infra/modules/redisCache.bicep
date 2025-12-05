// Azure Cache for Redis module
// Deploys an Azure Redis Cache instance for distributed caching

@description('The name of the Redis Cache instance')
param name string

@description('The Azure region for the Redis Cache')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('The SKU name of the Redis Cache (Basic, Standard, Premium)')
@allowed([
  'Basic'
  'Standard'
  'Premium'
])
param skuName string = 'Basic'

@description('The SKU capacity (family C: 0-6 for Basic/Standard, family P: 1-5 for Premium)')
@minValue(0)
@maxValue(6)
param skuCapacity int = 0

@description('Enable non-SSL port (not recommended for production)')
param enableNonSslPort bool = false

@description('Minimum TLS version')
@allowed([
  '1.0'
  '1.1'
  '1.2'
])
param minimumTlsVersion string = '1.2'

// Determine SKU family based on SKU name
var skuFamily = skuName == 'Premium' ? 'P' : 'C'

resource redisCache 'Microsoft.Cache/redis@2023-08-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    enableNonSslPort: enableNonSslPort
    minimumTlsVersion: minimumTlsVersion
    sku: {
      name: skuName
      family: skuFamily
      capacity: skuCapacity
    }
    redisConfiguration: {
      'maxmemory-policy': 'volatile-lru'
    }
    publicNetworkAccess: 'Enabled'
  }
}

@description('The resource ID of the Redis Cache')
output id string = redisCache.id

@description('The name of the Redis Cache')
output name string = redisCache.name

@description('The hostname of the Redis Cache')
output hostName string = redisCache.properties.hostName

@description('The SSL port of the Redis Cache')
output sslPort int = redisCache.properties.sslPort

@description('The primary connection string for the Redis Cache')
output connectionString string = '${redisCache.properties.hostName}:${redisCache.properties.sslPort},password=${redisCache.listKeys().primaryKey},ssl=True,abortConnect=False'

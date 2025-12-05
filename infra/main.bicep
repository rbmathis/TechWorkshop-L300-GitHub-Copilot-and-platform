// Main Bicep template orchestrator for ZavaStorefront infrastructure
// This template deploys all resources needed for the containerized web application

targetScope = 'subscription'

@minLength(1)
@description('The Azure region for all resources')
param location string = 'westus3'

@description('The environment name (e.g., dev, test, prod)')
@minLength(3)
@maxLength(10)
param environmentName string = 'dev'

@description('The application name prefix')
@minLength(3)
@maxLength(15)
param appNamePrefix string = 'zavastore'

@description('Tags to apply to all resources')
param tags object = {
  environment: environmentName
  application: 'ZavaStorefront'
  managedBy: 'Bicep'
}

@description('Deploy Azure Redis Cache for distributed caching')
param deployRedisCache bool = true

// Variables
var resourceGroupName = 'rg-${appNamePrefix}-${environmentName}-${location}'
var logAnalyticsName = 'law-${appNamePrefix}-${environmentName}-${location}'
var appInsightsName = 'appi-${appNamePrefix}-${environmentName}-${location}'
var acrName = 'acr${replace(appNamePrefix, '-', '')}${environmentName}'
var appServicePlanName = 'asp-${appNamePrefix}-${environmentName}-${location}'
var appServiceName = 'app-${appNamePrefix}-${environmentName}-${location}'
var aiFoundryName = 'aif-${appNamePrefix}-${environmentName}-${location}'
var redisCacheName = 'redis-${appNamePrefix}-${environmentName}'
var appConfigName = 'appconfig-${appNamePrefix}-${environmentName}'

// Resource Group
resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Log Analytics Workspace
module logAnalytics 'modules/logAnalytics.bicep' = {
  scope: rg
  name: 'logAnalytics-deployment'
  params: {
    name: logAnalyticsName
    location: location
    tags: tags
  }
}

// Application Insights
module appInsights 'modules/appInsights.bicep' = {
  scope: rg
  name: 'appInsights-deployment'
  params: {
    name: appInsightsName
    location: location
    tags: tags
    workspaceResourceId: logAnalytics.outputs.id
  }
}

// Azure Container Registry
module acr 'modules/acr.bicep' = {
  scope: rg
  name: 'acr-deployment'
  params: {
    name: acrName
    location: location
    tags: tags
  }
}

// App Service Plan (Linux)
module appServicePlan 'modules/appServicePlan.bicep' = {
  scope: rg
  name: 'appServicePlan-deployment'
  params: {
    name: appServicePlanName
    location: location
    tags: tags
  }
}

// Azure Cache for Redis (optional)
module redisCache 'modules/redisCache.bicep' = if (deployRedisCache) {
  scope: rg
  name: 'redisCache-deployment'
  params: {
    name: redisCacheName
    location: location
    tags: tags
    skuName: 'Basic'
    skuCapacity: 0
  }
}

// App Service
module appService 'modules/appService.bicep' = {
  scope: rg
  name: 'appService-deployment'
  params: {
    name: appServiceName
    location: location
    tags: union(tags, {
      'azd-service-name': 'web'
    })
    appServicePlanId: appServicePlan.outputs.id
    appInsightsConnectionString: appInsights.outputs.connectionString
    appInsightsInstrumentationKey: appInsights.outputs.instrumentationKey
    acrLoginServer: acr.outputs.loginServer
    redisConnectionString: deployRedisCache ? redisCache.outputs.connectionString : ''
  }
}

// Role Assignments (ACR Pull for App Service Managed Identity)
module roleAssignments 'modules/roleAssignments.bicep' = {
  scope: rg
  name: 'roleAssignments-deployment'
  params: {
    appServicePrincipalId: appService.outputs.principalId
    acrName: acrName
  }
}

// Azure AI Foundry
module aiFoundry 'modules/aiFoundry.bicep' = {
  scope: rg
  name: 'aiFoundry-deployment'
  params: {
    name: aiFoundryName
    location: location
    tags: tags
  }
}

// Azure App Configuration
module appConfiguration 'modules/appConfiguration.bicep' = {
  scope: rg
  name: 'appConfiguration-deployment'
  params: {
    appConfigName: appConfigName
    location: location
    tags: tags
  }
}

// Outputs
output resourceGroupName string = rg.name
output appServiceName string = appService.outputs.name
output appServiceHostName string = appService.outputs.defaultHostName
output acrName string = acr.outputs.name
output acrLoginServer string = acr.outputs.loginServer
output appInsightsConnectionString string = appInsights.outputs.connectionString
output appConfigName string = appConfiguration.outputs.appConfigName
output appConfigConnectionString string = appConfiguration.outputs.connectionString

// Service-specific outputs for azd
output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = acr.outputs.loginServer
output AZURE_CONTAINER_REGISTRY_NAME string = acr.outputs.name
output SERVICE_WEB_NAME string = appService.outputs.name
output SERVICE_WEB_RESOURCE_EXISTS bool = true
output SERVICE_WEB_ENDPOINT string = 'https://${appService.outputs.defaultHostName}'
output REDIS_CACHE_NAME string = deployRedisCache ? redisCache.outputs.name : ''
output REDIS_CACHE_HOSTNAME string = deployRedisCache ? redisCache.outputs.hostName : ''
output APP_CONFIG_NAME string = appConfiguration.outputs.appConfigName
output APP_CONFIG_ENDPOINT string = 'https://${appConfigName}.azconfig.io'

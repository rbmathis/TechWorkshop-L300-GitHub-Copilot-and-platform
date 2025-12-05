// Main Bicep template orchestrator for ZavaStorefront infrastructure
// This template deploys all resources needed for the containerized web application

targetScope = 'subscription'

@description('The environment name (e.g., dev, test, prod)')
@minLength(3)
@maxLength(10)
param environmentName string = 'dev'

@description('The Azure region for all resources')
@minLength(1)
param location string = 'westus3'

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

// Variables
var resourceGroupName = 'rg-${appNamePrefix}-${environmentName}-${location}'
var logAnalyticsName = 'law-${appNamePrefix}-${environmentName}-${location}'
var appInsightsName = 'appi-${appNamePrefix}-${environmentName}-${location}'
var acrName = 'acr${replace(appNamePrefix, '-', '')}${environmentName}'
var appServicePlanName = 'asp-${appNamePrefix}-${environmentName}-${location}'
var appServiceName = 'app-${appNamePrefix}-${environmentName}-${location}'
var aiFoundryName = 'aif-${appNamePrefix}-${environmentName}-${location}'

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

// Outputs
output resourceGroupName string = rg.name
output appServiceName string = appService.outputs.name
output appServiceHostName string = appService.outputs.defaultHostName
output acrName string = acr.outputs.name
output acrLoginServer string = acr.outputs.loginServer
output appInsightsConnectionString string = appInsights.outputs.connectionString

// Service-specific outputs for azd
output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = acr.outputs.loginServer
output AZURE_CONTAINER_REGISTRY_NAME string = acr.outputs.name
output SERVICE_WEB_NAME string = appService.outputs.name
output SERVICE_WEB_RESOURCE_EXISTS bool = true
output SERVICE_WEB_ENDPOINT string = 'https://${appService.outputs.defaultHostName}'

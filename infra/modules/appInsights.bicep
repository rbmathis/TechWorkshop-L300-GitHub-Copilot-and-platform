// Application Insights module
// Provides application performance monitoring and diagnostics

@description('The name of the Application Insights instance')
param name string

@description('The Azure region for the resource')
param location string

@description('Tags to apply to the resource')
param tags object = {}

@description('The resource ID of the Log Analytics workspace')
param workspaceResourceId string

@description('Application type')
@allowed([
  'web'
  'other'
])
param applicationType string = 'web'

@description('Daily data volume cap in GB (0 = no cap)')
@minValue(0)
param dailyDataCapInGB int = 5

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  tags: tags
  kind: applicationType
  properties: {
    Application_Type: applicationType
    Flow_Type: 'Bluefield'
    Request_Source: 'rest'
    WorkspaceResourceId: workspaceResourceId
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    DisableIpMasking: false
    DisableLocalAuth: false
  }
}

// Configure daily data cap if specified
resource dataCapSettings 'Microsoft.Insights/components/CurrentBillingFeatures@2015-05-01' = if (dailyDataCapInGB > 0) {
  parent: appInsights
  name: 'current'
  properties: {
    CurrentBillingFeatures: 'Basic'
    DataVolumeCap: {
      Cap: dailyDataCapInGB
      WarningThreshold: 90
      StopSendNotificationWhenHitCap: false
    }
  }
}

@description('The resource ID of Application Insights')
output id string = appInsights.id

@description('The name of Application Insights')
output name string = appInsights.name

@description('The instrumentation key for Application Insights')
output instrumentationKey string = appInsights.properties.InstrumentationKey

@description('The connection string for Application Insights')
output connectionString string = appInsights.properties.ConnectionString

@description('The application ID of Application Insights')
output applicationId string = appInsights.properties.AppId

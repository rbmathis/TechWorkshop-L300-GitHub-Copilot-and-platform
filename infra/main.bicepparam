using './main.bicep'

param environmentName = 'dev'
param location = 'westus3'
param appNamePrefix = 'zavastore'
param tags = {
  environment: 'dev'
  application: 'ZavaStorefront'
  managedBy: 'Bicep'
}

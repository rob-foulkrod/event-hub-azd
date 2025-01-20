targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

param ehServiceName string = ''

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string

// Tags that should be applied to all resources.
// 
// Note that 'azd-service-name' tags should be applied separately to service host resources.
// Example usage:
//   tags: union(tags, { 'azd-service-name': <service name in azure.yaml> })
var tags = {
  'azd-env-name': environmentName
}

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))

resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

// deploy the conferenceAPI app first, as it is required for the APIM deployment
module eventhub 'eventhub.bicep' = {
  scope: rg
  name: 'eventHub'
  params: {
    name: !empty(ehServiceName) ? ehServiceName : '${abbrs.apiManagementService}${resourceToken}'
    location: location
    tags: tags
  }
}

output EH-AZD-BLOB-CONNECTION string = eventhub.outputs.blobStorageConnectionString
output EH-AZD-EH-CONNECTION string = eventhub.outputs.eventHubNamespaceConnectionString


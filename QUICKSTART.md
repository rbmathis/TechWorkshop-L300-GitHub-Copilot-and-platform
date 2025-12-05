# Quick Start Guide - Azure Infrastructure Deployment

## Prerequisites
- Azure CLI installed
- Azure Developer CLI (azd) installed
- Active Azure subscription

## Deploy Infrastructure (First Time)

```bash
# 1. Login to Azure
az login
azd auth login

# 2. Initialize azd environment
azd init

# 3. Set environment variables when prompted:
#    - Environment name: dev
#    - Location: westus3

# 4. Deploy all infrastructure
azd up
```

## Verify Deployment

```bash
# Check environment variables
cat .azure/dev/.env

# List deployed resources
az resource list --resource-group rg-zavastore-dev-westus3 --output table
```

## Update Infrastructure

```bash
# After making changes to Bicep files
azd provision
```

## Access Deployed Resources

```bash
# Get App Service URL
azd env get-value APP_SERVICE_URL

# Get Azure OpenAI endpoint
azd env get-value AZURE_OPENAI_ENDPOINT

# Get Container Registry name
azd env get-value AZURE_CONTAINER_REGISTRY_NAME
```

## Delete All Resources

```bash
azd down
```

## Troubleshooting

### Validate Bicep before deployment
```bash
az deployment sub validate \
  --location westus3 \
  --template-file infra/main.bicep \
  --parameters infra/main.parameters.json \
  --parameters environmentName=dev location=westus3
```

### View deployment logs
```bash
az deployment sub show \
  --name <deployment-name> \
  --query properties.error
```

## Common Issues

1. **ACR name already taken**: Change `acrName` parameter in `main.parameters.json`
2. **GPT-4o not available**: Ensure you're deploying to westus3 or another supported region
3. **Quota exceeded**: Check your subscription limits for the services

## Cost Management

View estimated costs:
```bash
az consumption usage list --query "[].{Date:usageEnd, Cost:pretaxCost}" -o table
```

## Next Steps After Infrastructure Deployment

1. Build and push Docker image to ACR
2. Update App Service to use your custom image
3. Configure CI/CD pipeline
4. Test managed identity authentication
5. Monitor application in Application Insights

## Resources

- Documentation: See `infra/README.md`
- Azure Portal: https://portal.azure.com
- azd Documentation: https://learn.microsoft.com/azure/developer/azure-developer-cli/

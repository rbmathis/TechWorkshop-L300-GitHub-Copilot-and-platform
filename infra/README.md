# Azure Infrastructure for ZavaStorefront

This directory contains Azure infrastructure as code (IaC) using Bicep templates and Azure Developer CLI (azd) configuration.

## Structure

```
infra/
├── main.bicep                      # Root orchestrator template
├── main.parameters.json            # Environment-specific parameters
└── modules/
    ├── logAnalytics.bicep         # Log Analytics workspace
    ├── appInsights.bicep          # Application Insights
    ├── acr.bicep                  # Azure Container Registry
    ├── appServicePlan.bicep       # Linux App Service Plan
    ├── appService.bicep           # App Service with Managed Identity
    ├── roleAssignments.bicep      # RBAC role assignments
    └── aiFoundry.bicep            # Azure AI Foundry workspace
```

## Prerequisites

- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) (az)
- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd) (azd)
- An active Azure subscription

## Resources Provisioned

### Core Infrastructure
1. **Resource Group**: `rg-zavastore-dev-westus3`
2. **Log Analytics Workspace**: `law-zavastore-dev-westus3`
   - SKU: PerGB2018
   - Retention: 30 days
3. **Application Insights**: `appi-zavastore-dev-westus3`
   - Daily cap: 5GB
   - Adaptive sampling enabled

### Container Infrastructure
4. **Azure Container Registry**: `acrzavadev`
   - SKU: Basic
   - Admin access: Disabled (RBAC only)
5. **App Service Plan**: `asp-zavastore-dev-westus3`
   - SKU: B1 Basic (Linux)
6. **App Service**: `app-zavastore-dev-westus3`
   - Container-based hosting
   - System-assigned managed identity
   - Initial image: `mcr.microsoft.com/appsvc/staticsite:latest`

### AI Infrastructure
7. **Azure AI Foundry**: `aif-zavastore-dev-westus3`
   - Model: GPT-4o (`gpt-4o-2024-11-20`)
   - Capacity: 30K TPM

### Security
8. **Role Assignments**:
   - AcrPull: App Service → Container Registry
   - Cognitive Services User: App Service → AI Foundry

## Deployment

### Initial Setup

1. **Login to Azure**:
   ```bash
   az login
   azd auth login
   ```

2. **Initialize azd environment**:
   ```bash
   azd init
   ```
   - Environment name: `dev` (or your choice)
   - Location: `westus3` (or your choice)

3. **Deploy infrastructure**:
   ```bash
   azd up
   ```

### Subsequent Deployments

To update infrastructure after making changes:

```bash
azd provision
```

## Configuration

### Parameters

All parameters are defined in `infra/main.parameters.json`:

- `environmentName`: Environment name (e.g., "dev", "staging", "prod")
- `location`: Azure region (default: "westus3")
- `appName`: Application name (default: "zavastore")
- `appServicePlanSku`: App Service Plan SKU (default: "B1")
- `acrName`: Container Registry name (default: "acrzavadev")
- `acrSku`: Container Registry SKU (default: "Basic")
- `appInsightsDailyCap`: Daily data cap in GB (default: 5)
- `gptModelName`: GPT model name (default: "gpt-4o")
- `gptModelVersion`: GPT model version (default: "gpt-4o-2024-11-20")
- `gptModelCapacity`: TPM capacity (default: 30)
- `initialContainerImage`: Placeholder image

### Environment Variables

After deployment, azd will create `.azure/<env-name>/.env` with outputs:

- `AZURE_LOCATION`: Azure region
- `AZURE_RESOURCE_GROUP`: Resource group name
- `APPLICATIONINSIGHTS_CONNECTION_STRING`: App Insights connection string
- `AZURE_CONTAINER_REGISTRY_ENDPOINT`: ACR login server
- `AZURE_OPENAI_ENDPOINT`: Azure OpenAI endpoint
- `AZURE_OPENAI_API_KEY`: Azure OpenAI API key (backup)
- `AZURE_OPENAI_DEPLOYMENT_NAME`: Model deployment name
- `APP_SERVICE_NAME`: App Service name
- `APP_SERVICE_URL`: App Service URL

## Cost Estimate

Monthly cost for dev environment:

- App Service Plan B1: ~$13/month
- Container Registry Basic: ~$5/month
- Application Insights: ~$2-10/month (5GB cap)
- Azure AI Foundry: Pay-per-use (~$0.005/1K input tokens, ~$0.015/1K output tokens)

**Total Infrastructure**: ~$20-30/month + AI usage

## Managed Identity & RBAC

The App Service uses a system-assigned managed identity with the following roles:

1. **AcrPull** (7f951dda-4ed3-4680-a7ca-43fe172d538d)
   - Scope: Azure Container Registry
   - Purpose: Pull container images

2. **Cognitive Services User** (a97b65f3-24c7-4388-baec-2e87135dc908)
   - Scope: Azure AI Foundry workspace
   - Purpose: Access GPT-4o model

## Cleanup

To delete all resources:

```bash
azd down
```

This will remove the resource group and all contained resources.

## Troubleshooting

### Bicep Validation

Validate templates before deployment:

```bash
az deployment sub validate \
  --location westus3 \
  --template-file infra/main.bicep \
  --parameters infra/main.parameters.json \
  --parameters environmentName=dev location=westus3
```

### View Deployment Logs

```bash
azd monitor
```

### Common Issues

1. **ACR Name Conflict**: If `acrzavadev` is taken, change the `acrName` parameter
2. **Quota Issues**: Check subscription limits for App Service Plans and AI models
3. **Region Availability**: Ensure GPT-4o is available in your chosen region

## Next Steps

After infrastructure is provisioned:

1. Build and push Docker image to ACR
2. Update App Service container configuration
3. Configure GitHub Actions for CI/CD
4. Integrate Application Insights SDK in code
5. Test managed identity authentication

## References

- [Azure Developer CLI Documentation](https://learn.microsoft.com/azure/developer/azure-developer-cli/)
- [Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
- [Azure App Service Container Documentation](https://learn.microsoft.com/azure/app-service/configure-custom-container)
- [Azure OpenAI Service](https://learn.microsoft.com/azure/ai-services/openai/)

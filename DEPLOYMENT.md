# Azure Developer CLI Deployment Guide

This guide walks through deploying the ZavaStorefront application to Azure using Azure Developer CLI (azd) with App Service as a Linux container.

## Prerequisites

- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) installed
- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd) installed
- An active Azure subscription
- Docker installed (for local testing)

## Architecture

The infrastructure includes:

- **Azure Container Registry (ACR)**: Stores the containerized application images
- **App Service Plan**: Linux-based compute with B1 SKU
- **App Service**: Hosts the .NET 6 web application as a container
- **Log Analytics Workspace**: Centralized logging and monitoring
- **Application Insights**: Application performance monitoring
- **Azure AI Foundry**: AI model hosting and management
- **Managed Identity**: Secure authentication between App Service and ACR using RBAC (AcrPull role)

## Deployment Steps

### 1. Login to Azure

```bash
az login
azd auth login
```

### 2. Initialize Environment (First Time Only)

```bash
# Set your Azure subscription
azd env new dev

# Configure environment variables
azd env set AZURE_LOCATION westus3
```

### 3. Preview Infrastructure Changes

Before deploying, preview what resources will be created:

```bash
azd provision --preview
```

### 4. Deploy Infrastructure and Application

Deploy all resources and the application:

```bash
azd up
```

This command will:
1. Build the Docker container from `src/Dockerfile`
2. Push the image to Azure Container Registry
3. Provision all Azure resources via Bicep templates
4. Deploy the container to App Service
5. Configure Application Insights monitoring

### 5. Access Your Application

After deployment completes, azd will output the application URL:

```
Endpoint: https://app-zavastore-dev-westus3.azurewebsites.net
```

## Local Testing (Optional)

Test the Docker container locally before deploying:

```bash
cd src

# Build the Docker image
docker build -t zavastore:local .

# Run the container
docker run -d -p 8080:8080 --name zavastore-test zavastore:local

# Test the application
curl http://localhost:8080

# Stop and remove the container
docker stop zavastore-test
docker rm zavastore-test
```

## Managing the Application

### View Application Logs

```bash
azd monitor --logs
```

### Redeploy Application Only

```bash
azd deploy
```

### Update Infrastructure Only

```bash
azd provision
```

### Clean Up Resources

```bash
azd down
```

## Troubleshooting

### Container Won't Start

Check the application logs in the Azure Portal:
1. Navigate to your App Service
2. Go to "Log stream" or "Logs" under Monitoring
3. Check for startup errors

### ACR Authentication Issues

Verify the managed identity has AcrPull permissions:

```bash
az role assignment list --assignee <MANAGED_IDENTITY_PRINCIPAL_ID> --scope <ACR_RESOURCE_ID>
```

### Application Insights Not Receiving Data

Ensure the connection string is properly configured:

```bash
az webapp config appsettings list --name <APP_SERVICE_NAME> --resource-group <RESOURCE_GROUP>
```

## Security Best Practices

- ✅ Admin credentials disabled on ACR (RBAC only)
- ✅ Anonymous pull access disabled on ACR
- ✅ HTTPS only enforced on App Service
- ✅ Managed Identity used for ACR authentication
- ✅ Key Vault purge protection enabled
- ✅ TLS 1.2 minimum enforced
- ✅ Container runs as non-root user

## Cost Optimization

Current SKUs are optimized for development:
- **ACR**: Basic ($0.167/day)
- **App Service Plan**: B1 (~$13/month)
- **Log Analytics**: Pay-as-you-go with 30-day retention

For production, consider:
- ACR Standard or Premium for geo-replication
- App Service Plan P1V3 or higher for production workloads
- Configure autoscaling based on demand

## Next Steps

1. Set up CI/CD pipeline with GitHub Actions
2. Configure custom domain and SSL certificate
3. Enable Azure Key Vault for secrets management
4. Configure Application Insights alerts
5. Set up automated backups for App Service

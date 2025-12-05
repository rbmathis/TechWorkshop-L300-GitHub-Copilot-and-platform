# GitHub Actions Quickstart (Container to App Service)

Configure these before running the workflow:

1. Create a service principal for OIDC

```bash
az ad sp create-for-rbac \
  --name "zavastore-gha" \
  --role "Contributor" \
  --scopes "/subscriptions/<sub id>/resourceGroups/<rg>" \
  --sdk-auth
```

- Required permissions: App Service deploy + ACR pull. If you want tighter scopes, give `Website Contributor` (or `Contributor`) on the resource group and `AcrPull` on the ACR.
- Copy the JSON output for the secret below.

2. Add repository **secret**

- GitHub UI: Repo → Settings → Secrets and variables → Actions → New repository secret.
- Name: `AZURE_CREDENTIALS` ; Value: the JSON from the command above.

3. Add repository **variables**

- GitHub UI: Repo → Settings → Secrets and variables → Actions → Variables → New repository variable.
- `ACR_NAME`: e.g., `acrzavastoredev`
- `ACR_LOGIN_SERVER`: e.g., `acrzavastoredev.azurecr.io`
- `AZURE_WEBAPP_NAME`: e.g., `app-zavastore-dev-westus3`

Usage:

- Workflow: `.github/workflows/webapp-container.yml`
- Triggers on push to `main` or manual dispatch.
- Builds `./src` as a Docker image, pushes to ACR, then updates the App Service container to that tag (`${{ github.sha }}`).

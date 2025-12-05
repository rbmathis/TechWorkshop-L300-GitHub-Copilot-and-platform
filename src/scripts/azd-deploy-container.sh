#!/usr/bin/env bash
set -euo pipefail

# This hook builds the application container image remotely with ACR, pushes it,
# and wires the Azure App Service to the new image before azd performs the deploy step.

command -v azd >/dev/null 2>&1 || {
  echo "azd CLI is required for this hook" >&2
  exit 1
}

command -v az >/dev/null 2>&1 || {
  echo "Azure CLI (az) is required for this hook" >&2
  exit 1
}

# Ensure Azure CLI is authenticated (use azd's auth context if available)
if ! az account show >/dev/null 2>&1; then
  echo "Azure CLI not logged in. Attempting to authenticate..."
  # azd stores credentials that az can use via the same identity
  az login --use-device-code || {
    echo "Failed to authenticate Azure CLI. Please run 'az login' before 'azd up'." >&2
    exit 1
  }
fi

require_value() {
  local key="$1"
  local value
  value="$(azd env get-value "$key" 2>/dev/null || true)"
  if [[ -z "${value// }" ]]; then
    echo "Environment value '$key' is required but was not found. Run 'azd env set $key <value>'." >&2
    exit 1
  fi
  printf '%s' "$value"
}

ENV_NAME="$(require_value AZURE_ENV_NAME)"
REGISTRY_NAME="$(require_value AZURE_CONTAINER_REGISTRY_NAME)"
REGISTRY_ENDPOINT="$(require_value AZURE_CONTAINER_REGISTRY_ENDPOINT)"
RESOURCE_GROUP="$(require_value AZURE_RESOURCE_GROUP)"
WEB_APP_NAME="$(require_value SERVICE_WEB_NAME)"

TIMESTAMP="$(date -u +%Y%m%d%H%M%S)"
IMAGE_NAME="web:${ENV_NAME}-${TIMESTAMP}"
FULL_IMAGE="${REGISTRY_ENDPOINT}/${IMAGE_NAME}"

echo "Building container image '${FULL_IMAGE}' via Azure Container Registry..."
az acr build \
  --registry "${REGISTRY_NAME}" \
  --image "${IMAGE_NAME}" \
  --file ./Dockerfile \
  --platform linux/amd64 \
  .

echo "Updating azd environment with the latest container image reference..."
azd env set SERVICE_WEB_CONTAINER_IMAGE "${FULL_IMAGE}"

echo "Configuring App Service '${WEB_APP_NAME}' to run the new container image..."
az webapp config container set \
  --name "${WEB_APP_NAME}" \
  --resource-group "${RESOURCE_GROUP}" \
  --docker-custom-image-name "${FULL_IMAGE}" \
  --docker-registry-server-url "https://${REGISTRY_ENDPOINT}"

echo "Container deployment hook complete."

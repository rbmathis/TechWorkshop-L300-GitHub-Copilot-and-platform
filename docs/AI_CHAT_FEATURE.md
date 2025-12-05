# AI Chat Feature

## Overview

The AI Chat feature provides an interactive chat interface powered by Microsoft Foundry Phi4. Users can have natural conversations with the AI assistant about the Zava Storefront application.

## Feature Flag

This feature is controlled by a feature flag and is **disabled by default**. 

### Enabling the Feature

#### Option 1: Local Development (appsettings.local.json)

1. Copy `appsettings.local.json.example` to `appsettings.local.json`
2. Set the feature flag to `true`:
```json
{
  "FeatureManagement": {
    "AiChat": true
  }
}
```

#### Option 2: Azure App Configuration

The feature can be controlled centrally through Azure App Configuration:

1. Navigate to your Azure App Configuration resource
2. Add or update the feature flag: `FeatureManagement:AiChat`
3. Set the value to `true` or `false`

#### Option 3: appsettings.json (Production)

For permanent enablement in a specific environment:

```json
{
  "FeatureManagement": {
    "AiChat": true
  }
}
```

## Configuration

### Required Settings

The following settings must be configured for the chat feature to work:

```json
{
  "AzureAI": {
    "FoundryEndpoint": "https://your-foundry-endpoint.inference.ai.azure.com",
    "ModelName": "Phi-4"
  }
}
```

### Authentication

The feature uses `DefaultAzureCredential` for authentication, which supports:
- Managed Identity (recommended for Azure deployments)
- Azure CLI credentials (for local development)
- Visual Studio credentials (for local development)
- Environment variables

For local development, ensure you're logged in with Azure CLI:
```bash
az login
```

## Usage

Once enabled, users will see an "AI Chat" link in the navigation bar. Clicking it opens the chat interface where they can:

1. Type messages in the input box
2. View conversation history
3. Clear the conversation history using the "Clear History" button
4. See loading indicators while waiting for responses
5. Receive error notifications via Bootstrap toasts if something goes wrong

## Features

- **Session-based conversation**: Chat history is maintained during the user's session
- **Context-aware responses**: The AI maintains context from previous messages
- **Error handling**: Graceful error handling with user-friendly messages
- **Responsive UI**: Works on desktop and mobile devices
- **Accessibility**: Built with Bootstrap components for better accessibility

## Architecture

### Components

1. **ChatController**: MVC controller handling HTTP requests
2. **ChatService**: Service layer communicating with Azure AI Foundry
3. **Chat/Index.cshtml**: Razor view providing the UI
4. **ChatMessage**: Model representing individual messages
5. **ChatViewModel**: View model for the chat page

### Feature Flag Integration

- Controller checks feature flag before allowing access (returns 404 if disabled)
- Navigation link only appears when feature is enabled
- All endpoints (Index, SendMessage, ClearHistory) are protected

## Testing

The feature includes comprehensive unit tests:

- **ChatServiceTests**: Tests for the service layer (7 tests)
- **ChatControllerTests**: Tests for controller including feature flag behavior (7 tests)

Run tests with:
```bash
dotnet test
```

## Troubleshooting

### Chat link doesn't appear
- Ensure the feature flag is set to `true`
- Check that the application has been restarted after configuration changes

### "Chat service is not configured" error
- Verify the `AzureAI:FoundryEndpoint` is set correctly
- Ensure the endpoint URL is valid and accessible

### Authentication errors
- Check that managed identity is configured (in Azure)
- Verify Azure CLI login (for local development)
- Ensure the identity has appropriate permissions to access the Foundry endpoint

### Connection errors
- Verify network connectivity to the Foundry endpoint
- Check firewall rules if applicable
- Ensure the endpoint is deployed and running

## Security

- All API calls use HTTPS
- Authentication via Azure AD/Managed Identity
- No chat history is persisted to database (session only)
- Feature flag provides additional access control
- CodeQL security scan completed with 0 vulnerabilities

## Future Enhancements

Potential improvements for future releases:
- Persistent chat history storage
- User-specific chat sessions
- Export/download conversation history
- Streaming responses for better UX
- Support for multiple AI models
- Rate limiting
- Content moderation

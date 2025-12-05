using Azure;
using Azure.AI.Inference;
using Azure.Identity;
using ZavaStorefront.Models;

namespace ZavaStorefront.Services
{
    public class ChatService : IChatService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatService> _logger;
        private readonly ITelemetryClient _telemetry;

        public ChatService(
            IConfiguration configuration, 
            ILogger<ChatService> logger,
            ITelemetryClient telemetry)
        {
            _configuration = configuration;
            _logger = logger;
            _telemetry = telemetry;
        }

        public async Task<string> SendMessageAsync(string userMessage, List<ChatMessage> conversationHistory)
        {
            try
            {
                var endpoint = _configuration["AzureAI:FoundryEndpoint"];
                var modelName = _configuration["AzureAI:ModelName"] ?? "Phi-4";

                if (string.IsNullOrEmpty(endpoint))
                {
                    _logger.LogWarning("AzureAI:FoundryEndpoint is not configured");
                    return "Chat service is not configured. Please set the AzureAI:FoundryEndpoint configuration.";
                }

                _logger.LogInformation("Sending message to Foundry endpoint: {Endpoint}, Model: {Model}", endpoint, modelName);

                var credential = new DefaultAzureCredential();
                var client = new ChatCompletionsClient(new Uri(endpoint), credential);

                // Build the conversation context
                var chatMessages = new List<ChatRequestMessage>();
                
                // Add system message
                chatMessages.Add(new ChatRequestSystemMessage("You are a helpful AI assistant for the Zava Storefront application."));

                // Add conversation history
                foreach (var msg in conversationHistory)
                {
                    if (msg.Role.Equals("user", StringComparison.OrdinalIgnoreCase))
                    {
                        chatMessages.Add(new ChatRequestUserMessage(msg.Content));
                    }
                    else if (msg.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase))
                    {
                        chatMessages.Add(new ChatRequestAssistantMessage(msg.Content));
                    }
                }

                // Add the new user message
                chatMessages.Add(new ChatRequestUserMessage(userMessage));

                var requestOptions = new ChatCompletionsOptions()
                {
                    Messages = chatMessages,
                    Model = modelName,
                    Temperature = 0.7f,
                    MaxTokens = 800,
                    NucleusSamplingFactor = 0.95f
                };

                _telemetry.TrackEvent("ChatMessageSent", new Dictionary<string, string>
                {
                    { "modelName", modelName },
                    { "messageCount", chatMessages.Count.ToString() }
                });

                var response = await client.CompleteAsync(requestOptions);
                var assistantMessage = response.Value.Content;

                _telemetry.TrackEvent("ChatResponseReceived", new Dictionary<string, string>
                {
                    { "modelName", modelName },
                    { "responseLength", assistantMessage.Length.ToString() }
                });

                _logger.LogInformation("Received response from Foundry endpoint");

                return assistantMessage;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Azure AI request failed: {Message}", ex.Message);
                _telemetry.TrackEvent("ChatError", new Dictionary<string, string>
                {
                    { "errorType", "RequestFailedException" },
                    { "message", ex.Message }
                });
                return $"Error communicating with AI service: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to chat service: {Message}", ex.Message);
                _telemetry.TrackEvent("ChatError", new Dictionary<string, string>
                {
                    { "errorType", ex.GetType().Name },
                    { "message", ex.Message }
                });
                return $"An unexpected error occurred: {ex.Message}";
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using ZavaStorefront.Models;
using ZavaStorefront.Services;
using System.Text.Json;

namespace ZavaStorefront.Controllers
{
    public class ChatController : Controller
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IChatService _chatService;
        private readonly ISessionManager _sessionManager;

        public ChatController(
            ILogger<ChatController> logger, 
            IChatService chatService,
            ISessionManager sessionManager)
        {
            _logger = logger;
            _chatService = chatService;
            _sessionManager = sessionManager;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Loading chat page");
            
            var viewModel = new ChatViewModel
            {
                Messages = GetChatHistory()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, error = "Message cannot be empty" });
            }

            _logger.LogInformation("Sending chat message: {Message}", message);

            try
            {
                // Get conversation history
                var chatHistory = GetChatHistory();

                // Send message to AI service
                var response = await _chatService.SendMessageAsync(message, chatHistory);

                // Add user message to history
                var userMessage = new ChatMessage
                {
                    Role = "user",
                    Content = message,
                    Timestamp = DateTime.UtcNow
                };
                chatHistory.Add(userMessage);

                // Add assistant response to history
                var assistantMessage = new ChatMessage
                {
                    Role = "assistant",
                    Content = response,
                    Timestamp = DateTime.UtcNow
                };
                chatHistory.Add(assistantMessage);

                // Save updated history
                SaveChatHistory(chatHistory);

                return Json(new 
                { 
                    success = true, 
                    userMessage = new 
                    { 
                        role = userMessage.Role, 
                        content = userMessage.Content, 
                        timestamp = userMessage.Timestamp.ToString("o") 
                    },
                    assistantMessage = new 
                    { 
                        role = assistantMessage.Role, 
                        content = assistantMessage.Content, 
                        timestamp = assistantMessage.Timestamp.ToString("o") 
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message");
                return Json(new { success = false, error = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ClearHistory()
        {
            _logger.LogInformation("Clearing chat history");
            _sessionManager.Remove("ChatHistory");
            return Json(new { success = true });
        }

        private List<ChatMessage> GetChatHistory()
        {
            var json = _sessionManager.GetString("ChatHistory");
            if (string.IsNullOrEmpty(json))
            {
                return new List<ChatMessage>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<ChatMessage>>(json) ?? new List<ChatMessage>();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize chat history");
                return new List<ChatMessage>();
            }
        }

        private void SaveChatHistory(List<ChatMessage> messages)
        {
            var json = JsonSerializer.Serialize(messages);
            _sessionManager.SetString("ChatHistory", json);
        }
    }
}

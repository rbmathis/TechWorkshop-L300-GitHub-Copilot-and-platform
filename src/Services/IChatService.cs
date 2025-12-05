using ZavaStorefront.Models;

namespace ZavaStorefront.Services
{
    public interface IChatService
    {
        Task<string> SendMessageAsync(string userMessage, List<ChatMessage> conversationHistory);
    }
}

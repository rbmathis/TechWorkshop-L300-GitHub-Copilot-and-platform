namespace ZavaStorefront.Models
{
    public class ChatViewModel
    {
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public string? ErrorMessage { get; set; }
    }
}

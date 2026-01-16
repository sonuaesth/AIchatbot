using AiChat.Backend.Domain.Enums;

namespace AiChat.Backend.Domain.Entities
{
    public class Chat
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Title { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public List<Message> Messages { get; set; } = new();
    }
}
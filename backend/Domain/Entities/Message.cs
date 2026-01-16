using AiChat.Backend.Domain.Enums;

namespace AiChat.Backend.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public ChatRole Role { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Chat? Chat { get; set; }
}
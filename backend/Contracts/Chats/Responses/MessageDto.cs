namespace AiChat.Backend.Contracts.Chats.Responses;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public string? Role { get; set; }
    public string? Text { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

}
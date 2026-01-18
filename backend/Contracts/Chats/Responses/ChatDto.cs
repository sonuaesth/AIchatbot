namespace AiChat.Backend.Contracts.Chats.Responses;

public class ChatDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Title { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
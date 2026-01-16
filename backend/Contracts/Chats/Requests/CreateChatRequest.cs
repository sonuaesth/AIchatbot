using System.ComponentModel.DataAnnotations;

namespace AiChat.Backend.Contracts.Chats.Requests;

public class CreateChatRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [MaxLength(200)]
    public string? Title { get; set; }
}
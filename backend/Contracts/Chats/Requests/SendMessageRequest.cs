using System.ComponentModel.DataAnnotations;

namespace AiChat.Backend.Contracts.Chats.Requests;

public class SendMessageRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(4000)]
    public string Text { get; set; } = string.Empty;
}
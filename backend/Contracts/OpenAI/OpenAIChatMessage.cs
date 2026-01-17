namespace AiChat.Backend.Contracts.OpenAI;

public class OpenAIChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
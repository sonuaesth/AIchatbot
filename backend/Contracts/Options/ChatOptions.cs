namespace AiChat.Backend.Contracts.Options;

public class ChatOptions
{
    public const string SectionName = "Chat";
    public string SystemPrompt { get; set; } = "You are a helpful assistant.";
    public int MaxHistoryMessages { get; set; } = 20;
    public double Temperature { get; set; } = 0.7;
}
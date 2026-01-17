namespace AiChat.Backend.Contracts.Options;

public class OpenAIOptions
{
    public const string SectionName = "OpenAI";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4.1-mini";
    public int TimeoutSeconds { get; set; } = 60;
}
namespace AiChat.Backend.Contracts.OpenAI;

public class OpenAIChatCompletionRequest
{
    public string Model { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.7;
    public List<OpenAIChatMessage> Messages { get; set; } = new();
}
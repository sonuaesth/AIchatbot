using System.Text.Json.Serialization;

namespace AiChat.Backend.Contracts.OpenAI;

public class OpenAIChatCompletionResponse
{
    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = new();

    public class Choice
    {
        [JsonPropertyName("message")]
        public OpenAIChatMessage Message { get; set; } = new();
    }
}
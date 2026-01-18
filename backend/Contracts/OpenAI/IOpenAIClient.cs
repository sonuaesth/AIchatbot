namespace AiChat.Backend.Contracts.OpenAI;

public interface IOpenAIClient
{
    Task<string> GetChatCompletionAsync(
        string systemPrompt,
        IReadOnlyList<OpenAIChatMessage> messages,
        CancellationToken ct = default);
}
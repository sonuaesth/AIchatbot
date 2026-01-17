using System.Net.Http.Json;
using AiChat.Backend.Contracts.OpenAI;
using AiChat.Backend.Contracts.Options;
using Microsoft.Extensions.Options;

namespace AiChat.Backend.Services.OpenAI;

public class OpenAIClient : IOpenAIClient
{
    private readonly HttpClient _http;
    private readonly OpenAIOptions _openAi;
    private readonly ChatOptions _chat;

    public OpenAIClient(
        IHttpClientFactory httpClientFactory,
        IOptions<OpenAIOptions> openAiOptions,
        IOptions<ChatOptions> chatOptions
    )
    {
        _http = httpClientFactory.CreateClient("OpenAI");
        _openAi = openAiOptions.Value;
        _chat = chatOptions.Value;
    }

    public async Task<string> GetChatCompletionAsync(
        string systemPrompt,
        IReadOnlyList<OpenAIChatMessage> messages,
        CancellationToken ct = default)
    {
        var payload = new OpenAIChatCompletionRequest
        {
            Model = _openAi.Model,
            Temperature = _chat.Temperature,
            Messages = new List<OpenAIChatMessage>
            {
                new() { Role = "system", Content = systemPrompt },
            },
            
        };
        payload.Messages.AddRange(messages);

        //POST /chat/completions
        var response = await _http.PostAsJsonAsync("chat/completions", payload, ct);
        if(!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"OpenAI API returned error: {errorBody}");
        }
        var data = await response.Content.ReadFromJsonAsync<OpenAIChatCompletionResponse>(cancellationToken: ct);
        var text = data?.Choices.FirstOrDefault()?.Message?.Content;

        if(string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("OpenAI API returned empty response");
        }
        return text.Trim();
    }
}
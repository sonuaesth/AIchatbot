using System.Net.Http.Json;
using System.Text.Json;
using System.Linq;
using System.Runtime.CompilerServices;
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

    public async IAsyncEnumerable<string> StreamChatCompletionAsync(
        string systemPrompt,
        IReadOnlyList<OpenAIChatMessage> messages,
        [EnumeratorCancellation] CancellationToken ct = default)
    {

        var outMessages = new List<object>
        {
            new
            {
                role = "system",
                content = systemPrompt
            }
        };

        foreach (var m in messages)
        {
            outMessages.Add(new
            {
                role = m.Role,
                content = m.Content
            });
        }

        var payload = new
        {
            model = _openAi.Model,
            temperature = _chat.Temperature,
            stream = true,
            messages = outMessages,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Content = JsonContent.Create(payload);

        using var response = await _http.SendAsync
            (request, 
            HttpCompletionOption.ResponseHeadersRead, 
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"OpenAI API returned error: {err}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            if(!line.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue;

            var json = line.Substring("data: ".Length).Trim();
            
            if (json == "[DONE]") yield break;

            JsonDocument doc;
            try {
                doc = JsonDocument.Parse(json);
            }
            catch
            {
                continue;
            }
            
            using (doc)
            {
                var root = doc.RootElement;
                if (!root.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0) continue;
                
                var choice0 = choices[0];
                if (!choice0.TryGetProperty("delta", out var deltaObj) ) continue;
                
                if(deltaObj.TryGetProperty("content", out var content))
                {
                    var piece = content.GetString();
                    if(!string.IsNullOrEmpty(piece)) yield return piece;
                }
            }
        }
    }
}

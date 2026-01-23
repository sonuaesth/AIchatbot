namespace AiChat.Backend.Contracts.Chats.Responses;

public record StreamEvent(
    string Type,
    string? Text = null,
    string? Error = null
);

using AiChat.Backend.Contracts.Chats.Requests;
using AiChat.Backend.Contracts.Chats.Responses;

namespace AiChat.Backend.Contracts.Chats;

public interface IChatService
{
    Task<ChatDto> CreateChatAsync(CreateChatRequest request);
    Task<IReadOnlyList<ChatDto>> GetChatsAsync(Guid userId);
    Task<IReadOnlyList<MessageDto>> GetMessagesAsync(Guid chatId, Guid userId);
    Task<MessageDto> SendMessageAsync(Guid chatId, SendMessageRequest request);
}
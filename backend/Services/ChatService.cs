using AiChat.Backend.Contracts.Chats;
using AiChat.Backend.Contracts.Chats.Requests;
using AiChat.Backend.Contracts.Chats.Responses;
using AiChat.Backend.Contracts.OpenAI;
using AiChat.Backend.Contracts.Options;
using AiChat.Backend.Domain.Entities;
using AiChat.Backend.Domain.Enums;
using AiChat.Backend.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AiChat.Backend.Services;

public class ChatService : IChatService
{
    private readonly IOpenAIClient _openAIClient;
    private readonly AppDbContext _db;
    private readonly ChatOptions _chatOptions;
    public ChatService(AppDbContext db, IOpenAIClient openAiClient, IOptions<ChatOptions> chatOptions)
    {
        _db = db;
        _openAIClient = openAiClient;
        _chatOptions = chatOptions.Value;
    }

    public async Task<ChatDto> CreateChatAsync(CreateChatRequest request)
    {
        if(request.UserId == Guid.Empty)
        {
            throw new Exception("User id is required");
        }
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = string.IsNullOrEmpty(request.Title) ? "New Chat" : request.Title.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Chats.Add(chat);
        await _db.SaveChangesAsync();
        return ToChatDto(chat);
    }

    public async Task<IReadOnlyList<ChatDto>> GetChatsAsync(Guid userId)
    {
        if(userId == Guid.Empty)
        {
            throw new Exception("User id is required");
        }
        var chats = await _db.Chats
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ChatDto
            {
                Id = x.Id,
                UserId = x.UserId,
                Title = x.Title,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
        return chats;
    }

    public async Task<IReadOnlyList<MessageDto>> GetMessagesAsync(Guid chatId, Guid userId)
    {
        if(chatId == Guid.Empty)throw new Exception("Chat id is required");
        if(userId == Guid.Empty)throw new Exception("User id is required");

        var chatExists = await _db.Chats
            .AsNoTracking()
            .AnyAsync(x => x.Id == chatId && x.UserId == userId);
        if(!chatExists)throw new Exception("Chat not found");

        var messages = await _db.Messages
            .AsNoTracking()
            .Where(x => x.ChatId == chatId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new MessageDto
            {
                Id = x.Id,
                ChatId = x.ChatId,
                Role = x.Role.ToString(),
                Text = x.Text,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
        return messages;
    }

    public async Task<MessageDto> SendMessageAsync(Guid chatId, SendMessageRequest request)
    {
        if(chatId == Guid.Empty)throw new Exception("Chat id is required");
        if(request.UserId == Guid.Empty)throw new Exception("User id is required");
        if(string.IsNullOrEmpty(request.Text))throw new Exception("Text is required");

        var chat = await _db.Chats
            .FirstOrDefaultAsync(x => x.Id == chatId && x.UserId == request.UserId);
        if(chat == null)throw new Exception("Chat not found");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            Role = ChatRole.User,
            Text = request.Text.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Messages.Add(message);
        if (string.IsNullOrWhiteSpace(chat.Title))
        {
            chat.Title = MakeTitleFromText(message.Text);
        }
        await _db.SaveChangesAsync();
        return ToMessageDto(message);
    }

    public async Task<MessageDto> SendAndReplyAsync(Guid chatId, SendMessageRequest request)
    {
        if(chatId == Guid.Empty)throw new ArgumentException("Chat id is required");
        if(request.UserId == Guid.Empty)throw new ArgumentException("User id is required");
        if(string.IsNullOrEmpty(request.Text))throw new ArgumentException("Text is required");

        var chat = await _db.Chats
            .FirstOrDefaultAsync(x => x.Id == chatId && x.UserId == request.UserId);
        if(chat == null)throw new KeyNotFoundException("Chat not found");

        var userMsg = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            Role = ChatRole.User,
            Text = request.Text.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Messages.Add(userMsg);
        
        if(string.IsNullOrWhiteSpace(chat.Title))
        {
            chat.Title = MakeTitleFromText(userMsg.Text);
        }
        await _db.SaveChangesAsync();

        var history = await _db.Messages
            .AsNoTracking()
            .Where(x => x.ChatId == chatId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(_chatOptions.MaxHistoryMessages)
            .Select(x => new OpenAIChatMessage
            {
                Role = x.Role == ChatRole.User ? "user" 
                :x.Role == ChatRole.Assistant ? "assistant"
                : "system",
                Content = x.Text
            })
            .ToListAsync();

        history.Reverse();

        var assistantText = await _openAIClient.GetChatCompletionAsync(
            systemPrompt: _chatOptions.SystemPrompt,
            messages: history
            );

        var assistantMsg = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            Role = ChatRole.Assistant,
            Text = assistantText,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Messages.Add(assistantMsg);
        await _db.SaveChangesAsync();
        return ToMessageDto(assistantMsg);
        

    }

    private static ChatDto ToChatDto(Chat chat)
    {
        return new ChatDto
        {
            Id = chat.Id,
            UserId = chat.UserId,
            Title = chat.Title,
            CreatedAt = chat.CreatedAt
        };
    }

    private static MessageDto ToMessageDto(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            ChatId = message.ChatId,
            Role = message.Role.ToString(),
            Text = message.Text,
            CreatedAt = message.CreatedAt
        };
    }

    private static string MakeTitleFromText(string text)
    {
        const int max = 40;
        var t = text.Trim();
        if (t.Length <= max) return t;
        return t.Substring(0, max).Trim() + "…";
    }
}

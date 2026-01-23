using AiChat.Backend.Contracts.Chats;
using AiChat.Backend.Contracts.Chats.Requests;
using AiChat.Backend.Contracts.Chats.Responses;

using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.Backend.Controllers;

[ApiController]
[Route("api/chats")]
public class ChatsController : ControllerBase
{
    private readonly IChatService _chatService;
    public ChatsController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
    {
        if(!ModelState.IsValid) return ValidationProblem(ModelState);

        try
        {
            var chat = await _chatService.CreateChatAsync(request);
            return CreatedAtAction(nameof(GetMessages), new { chatId = chat.Id, userId = request.UserId }, chat);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetChats([FromQuery] Guid userId)
    {
        try
        {
            var chats = await _chatService.GetChatsAsync(userId);
            return Ok(chats);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{chatId:guid}/messages")]
    public async Task<IActionResult> GetMessages([FromRoute] Guid chatId, [FromQuery] Guid userId)
    {
        try
        {
            var messages = await _chatService.GetMessagesAsync(chatId, userId);
            return Ok(messages);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{chatId:guid}/message")]
    public async Task<IActionResult> SendMessage([FromRoute] Guid chatId, [FromBody] SendMessageRequest request)
    {

        if(!ModelState.IsValid) return ValidationProblem(ModelState);

        try
        {
            var message = await _chatService.SendAndReplyAsync(chatId, request);
            return Ok(message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{chatId:guid}/message/stream")]
    public async Task SendMessageStream(
        [FromRoute] Guid chatId, 
        [FromBody] SendMessageRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return;
        }

        Response.StatusCode = 200;
        Response.ContentType = "application/x-ndjson; charset=utf-8";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";

        try
        {
            await foreach (var evt in _chatService.SendAndReplyStreamAsync(chatId, request, ct))
            {
                var line = JsonSerializer.Serialize(evt) + "\n";
                var bytes = Encoding.UTF8.GetBytes(line);

                await Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes($"{JsonSerializer.Serialize(evt)}\n"));
                await Response.BodyWriter.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            var err = new StreamEvent("error", Error: ex.Message);
            var line = JsonSerializer.Serialize(err) + "\n";
            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(line), ct);
            await Response.Body.FlushAsync(ct);
        }
    }

}

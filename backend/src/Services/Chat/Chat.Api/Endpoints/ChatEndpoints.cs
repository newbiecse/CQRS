using System.Text.Json;
using Chat.Application.Abstractions;
using Chat.Application.Models;

namespace Chat.Api.Endpoints;

internal static class ChatEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void MapChatEndpoints(this WebApplication app)
    {
        app.MapPost("/api/chat/completions", HandleChatCompletionsAsync);
    }

    private static async Task HandleChatCompletionsAsync(
        OpenAiChatRequest request,
        IChatAgentService agent,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var messages = request.Messages?
            .Where(m => !string.IsNullOrWhiteSpace(m.Content))
            .Select(m => new ChatMessage(m.Role ?? "user", m.Content!))
            .ToList() ?? [];

        if (messages.Count == 0)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new { message = "messages are required." }, cancellationToken);
            return;
        }

        if (request.Stream == true)
        {
            httpContext.Response.Headers.CacheControl = "no-cache";
            httpContext.Response.Headers.Connection = "keep-alive";
            httpContext.Response.ContentType = "text/event-stream";

            var id = $"chatcmpl-{Guid.NewGuid():N}";
            await foreach (var chunk in agent.StreamReplyAsync(messages, cancellationToken))
            {
                var payload = new
                {
                    id,
                    @object = "chat.completion.chunk",
                    choices = new[]
                    {
                        new { index = 0, delta = new { content = chunk }, finish_reason = (string?)null }
                    }
                };
                await httpContext.Response.WriteAsync(
                    $"data: {JsonSerializer.Serialize(payload, JsonOptions)}\n\n",
                    cancellationToken);
                await httpContext.Response.Body.FlushAsync(cancellationToken);
            }

            await httpContext.Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
            return;
        }

        var reply = await agent.GenerateReplyAsync(messages, cancellationToken);
        await httpContext.Response.WriteAsJsonAsync(new
        {
            id = $"chatcmpl-{Guid.NewGuid():N}",
            choices = new[]
            {
                new
                {
                    index = 0,
                    message = new { role = "assistant", content = reply },
                    finish_reason = "stop"
                }
            }
        }, cancellationToken);
    }

    internal sealed class OpenAiChatRequest
    {
        public List<OpenAiChatMessage>? Messages { get; set; }
        public string? Model { get; set; }
        public bool? Stream { get; set; }
    }

    internal sealed class OpenAiChatMessage
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
    }
}

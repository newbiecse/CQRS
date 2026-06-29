namespace Chat.Application.Models;

public sealed record ChatMessage(string Role, string Content);

public sealed record ChatCompletionRequest(
    IReadOnlyList<ChatMessage> Messages,
    string? Model = null,
    bool Stream = false);

public sealed record ProductContextItem(Guid Id, string Name, decimal Price);

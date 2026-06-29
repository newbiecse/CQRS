using Chat.Application.Models;

namespace Chat.Application.Abstractions;

public interface IChatAgentService
{
    Task<string> GenerateReplyAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> StreamReplyAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default);
}

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Chat.Application.Abstractions;
using Chat.Application.Models;
using Chat.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chat.Infrastructure.Agents;

public sealed class ShopChatAgentService(
    IShopContextProvider shopContext,
    IHttpClientFactory httpClientFactory,
    IOptions<ChatAgentOptions> options,
    ILogger<ShopChatAgentService> logger) : IChatAgentService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<string> GenerateReplyAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default)
    {
        var reply = string.Empty;
        await foreach (var chunk in StreamReplyAsync(messages, cancellationToken))
            reply += chunk;
        return reply;
    }

    public async IAsyncEnumerable<string> StreamReplyAsync(
        IReadOnlyList<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var userMessage = messages.LastOrDefault(m => m.Role == "user")?.Content ?? string.Empty;
        var systemPrompt = await BuildSystemPromptAsync(userMessage, cancellationToken);

        if (!string.IsNullOrWhiteSpace(options.Value.OpenAiApiKey))
        {
            await foreach (var chunk in StreamFromOpenAiAsync(systemPrompt, messages, cancellationToken))
                yield return chunk;
            yield break;
        }

        var fallback = await BuildFallbackReplyAsync(userMessage, cancellationToken);
        foreach (var chunk in ChunkText(fallback))
        {
            yield return chunk;
            await Task.Delay(12, cancellationToken);
        }
    }

    private async Task<string> BuildSystemPromptAsync(string userMessage, CancellationToken cancellationToken)
    {
        var relevant = await shopContext.SearchProductsAsync(userMessage, cancellationToken);
        if (relevant.Count == 0)
            relevant = await shopContext.GetCatalogAsync(cancellationToken);

        var catalogLines = relevant.Take(12)
            .Select(p => $"- {p.Name} (${p.Price:F2}) [id: {p.Id}]");

        return $"""
            You are {options.Value.AgentName}, a helpful shopping assistant for CQRS Shop.
            Answer in the same language the customer uses. Be concise and friendly.
            Use only the product catalog below when recommending items. If unsure, ask a clarifying question.
            When listing products, include name and price. Suggest browsing /products for the full catalog.

            Available products:
            {string.Join(Environment.NewLine, catalogLines)}
            """;
    }

    private async IAsyncEnumerable<string> StreamFromOpenAiAsync(
        string systemPrompt,
        IReadOnlyList<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var config = options.Value;
        var client = httpClientFactory.CreateClient("openai-chat");

        var payload = new
        {
            model = config.Model,
            stream = true,
            messages = new[]
            {
                new { role = "system", content = systemPrompt }
            }.Concat(messages.Select(m => new { role = m.Role, content = m.Content }))
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{config.OpenAiEndpoint.TrimEnd('/')}/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.OpenAiApiKey);

        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("OpenAI chat failed: {Status} {Body}", response.StatusCode, error);
            var fallback = await BuildFallbackReplyAsync(
                messages.LastOrDefault(m => m.Role == "user")?.Content ?? string.Empty,
                cancellationToken);
            foreach (var chunk in ChunkText(fallback))
                yield return chunk;
            yield break;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ", StringComparison.Ordinal))
                continue;

            var data = line["data: ".Length..].Trim();
            if (data == "[DONE]") break;

            OpenAiStreamChunk? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<OpenAiStreamChunk>(data, JsonOptions);
            }
            catch
            {
                continue;
            }

            var text = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;
            if (!string.IsNullOrEmpty(text))
                yield return text;
        }
    }

    private async Task<string> BuildFallbackReplyAsync(string userMessage, CancellationToken cancellationToken)
    {
        var trimmed = userMessage.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return $"Hi! I'm {options.Value.AgentName}. Ask me about products, prices, or search by name.";
        }

        if (IsGreeting(trimmed))
        {
            return $"Hello! I'm {options.Value.AgentName}. I can help you find products and compare prices. What are you looking for today?";
        }

        var matches = await shopContext.SearchProductsAsync(trimmed, cancellationToken);
        if (matches.Count > 0)
        {
            var lines = matches.Take(6)
                .Select((p, i) => $"{i + 1}. **{p.Name}** — ${p.Price:F2}");
            return $"Here are products that match your request:\n\n{string.Join("\n", lines)}\n\nOpen `/products/{matches[0].Id}` for details or browse `/products`.";
        }

        var catalog = await shopContext.GetCatalogAsync(cancellationToken);
        if (catalog.Count == 0)
            return "Our catalog is empty right now. Please check back later.";

        var sample = catalog.Take(5)
            .Select(p => $"- {p.Name} (${p.Price:F2})");
        return $"I couldn't find an exact match for \"{trimmed}\". Here are popular items:\n\n{string.Join("\n", sample)}\n\nTry searching on the home page or visit `/products`.";
    }

    private static bool IsGreeting(string text)
    {
        var lower = text.ToLowerInvariant();
        return lower is "hi" or "hello" or "hey" or "xin chào" or "chào"
            || lower.StartsWith("hi ")
            || lower.StartsWith("hello ");
    }

    private static IEnumerable<string> ChunkText(string text)
    {
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            yield return word + " ";
    }

    private sealed class OpenAiStreamChunk
    {
        [JsonPropertyName("choices")]
        public List<OpenAiChoice>? Choices { get; set; }
    }

    private sealed class OpenAiChoice
    {
        [JsonPropertyName("delta")]
        public OpenAiDelta? Delta { get; set; }
    }

    private sealed class OpenAiDelta
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}

using CqrsDemo.BuildingBlocks.Auth;
using CqrsDemo.BuildingBlocks.RateLimiting;
using Microsoft.Extensions.Options;
using Shop.Admin.Api.Options;

namespace Shop.Admin.Api.Endpoints;

internal static class ChatEndpoints
{
    public static void MapChatEndpoints(this WebApplication app)
    {
        app.MapPost("/api/chat/completions", ProxyChatCompletionsAsync)
            .RequireAuthorization(PlatformPolicies.Authenticated)
            .RequireRateLimiting(PlatformRateLimitPolicies.Chat);
    }

    private static async Task ProxyChatCompletionsAsync(
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        IOptions<AdminShopServiceOptions> options,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("admin-backend");
        var targetUrl = $"{options.Value.ChatApi.TrimEnd('/')}/api/chat/completions";

        using var request = new HttpRequestMessage(HttpMethod.Post, targetUrl)
        {
            Content = new StreamContent(context.Request.Body)
        };

        if (context.Request.ContentType is not null)
            request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(context.Request.ContentType);

        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        context.Response.StatusCode = (int)response.StatusCode;

        if (response.Content.Headers.ContentType is not null)
            context.Response.ContentType = response.Content.Headers.ContentType.ToString();

        foreach (var header in response.Headers)
            context.Response.Headers[header.Key] = header.Value.ToArray();

        foreach (var header in response.Content.Headers)
            context.Response.Headers[header.Key] = header.Value.ToArray();

        context.Response.Headers.Remove("transfer-encoding");
        await response.Content.CopyToAsync(context.Response.Body, cancellationToken);
    }
}

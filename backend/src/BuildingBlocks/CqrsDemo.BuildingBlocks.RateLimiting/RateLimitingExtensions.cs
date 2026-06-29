using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsDemo.BuildingBlocks.RateLimiting;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddPlatformRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>()
            ?? new RateLimitOptions();

        services.Configure<RateLimitOptions>(configuration.GetSection(RateLimitOptions.SectionName));

        if (!options.Enabled)
            return services;

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new { message = "Too many requests. Please try again later." },
                    cancellationToken);
            };

            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                CreateFixedWindow(
                    ResolvePartitionKey(httpContext, "global"),
                    options.PermitLimit,
                    options.WindowSeconds));

            limiter.AddPolicy(PlatformRateLimitPolicies.Auth, httpContext =>
                CreateFixedWindow(
                    ResolvePartitionKey(httpContext, "auth"),
                    options.AuthPermitLimit,
                    options.WindowSeconds));

            limiter.AddPolicy(PlatformRateLimitPolicies.Chat, httpContext =>
                CreateFixedWindow(
                    ResolvePartitionKey(httpContext, "chat"),
                    options.ChatPermitLimit,
                    options.WindowSeconds));

            limiter.AddPolicy(PlatformRateLimitPolicies.Write, httpContext =>
                CreateFixedWindow(
                    ResolvePartitionKey(httpContext, "write"),
                    options.WritePermitLimit,
                    options.WindowSeconds));
        });

        return services;
    }

    public static WebApplication UsePlatformRateLimiting(this WebApplication app)
    {
        var options = app.Configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>()
            ?? new RateLimitOptions();

        if (!options.Enabled)
            return app;

        app.UseRateLimiter();
        return app;
    }

    private static RateLimitPartition<string> CreateFixedWindow(
        string partitionKey,
        int permitLimit,
        int windowSeconds) =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });

    private static string ResolvePartitionKey(HttpContext context, string policy)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");

        if (!string.IsNullOrWhiteSpace(userId))
            return $"{policy}:user:{userId}";

        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            var clientIp = forwarded.Split(',')[0].Trim();
            return $"{policy}:ip:{clientIp}";
        }

        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"{policy}:ip:{remoteIp}";
    }
}

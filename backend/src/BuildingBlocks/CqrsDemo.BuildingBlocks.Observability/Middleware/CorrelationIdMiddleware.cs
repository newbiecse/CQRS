using System.Diagnostics;
using CqrsDemo.Contracts.Messaging;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace CqrsDemo.BuildingBlocks.Observability.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationHeaders.CorrelationId].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = CorrelationContext.GetOrCreateCorrelationId();

        var traceId = context.Request.Headers[CorrelationHeaders.TraceId].FirstOrDefault()
            ?? Activity.Current?.TraceId.ToString();

        context.Request.Headers[CorrelationHeaders.CorrelationId] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationHeaders.CorrelationId] = correlationId;
            return Task.CompletedTask;
        });

        using (CorrelationContext.BeginScope(correlationId, traceId))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            if (traceId is not null)
            {
                using (LogContext.PushProperty("TraceId", traceId))
                    await next(context);
            }
            else
            {
                await next(context);
            }
        }
    }
}

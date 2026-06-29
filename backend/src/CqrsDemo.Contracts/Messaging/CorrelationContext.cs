using System.Diagnostics;

namespace CqrsDemo.Contracts.Messaging;

public static class CorrelationContext
{
    private static readonly AsyncLocal<string?> CurrentCorrelationId = new();
    private static readonly AsyncLocal<string?> CurrentTraceId = new();

    public static string? CorrelationId
    {
        get => CurrentCorrelationId.Value;
        set => CurrentCorrelationId.Value = value;
    }

    public static string? TraceId
    {
        get => CurrentTraceId.Value;
        set => CurrentTraceId.Value = value;
    }

    public static string GetOrCreateCorrelationId() =>
        CorrelationId ??= Guid.NewGuid().ToString("N");

    public static IDisposable BeginScope(string? correlationId, string? traceId)
    {
        var previousCorrelationId = CorrelationId;
        var previousTraceId = TraceId;

        if (!string.IsNullOrWhiteSpace(correlationId))
            CorrelationId = correlationId;

        if (!string.IsNullOrWhiteSpace(traceId))
            TraceId = traceId;
        else if (Activity.Current is { } activity)
            TraceId = activity.TraceId.ToString();

        return new CorrelationScope(previousCorrelationId, previousTraceId);
    }

    private sealed class CorrelationScope(string? previousCorrelationId, string? previousTraceId) : IDisposable
    {
        public void Dispose()
        {
            CorrelationId = previousCorrelationId;
            TraceId = previousTraceId;
        }
    }
}

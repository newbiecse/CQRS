namespace CqrsDemo.Contracts.Messaging;

public static class CorrelationHeaders
{
    public const string CorrelationId = "X-Correlation-Id";
    public const string TraceId = "X-Trace-Id";
}

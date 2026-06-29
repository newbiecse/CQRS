using System.Diagnostics;

namespace CqrsDemo.Contracts.Messaging;

public sealed record IntegrationEventEnvelope(
    string EventType,
    string Payload,
    string? CorrelationId = null,
    string? TraceId = null)
{
    public static IntegrationEventEnvelope Create(string eventType, string payload) =>
        new(
            eventType,
            payload,
            CorrelationContext.CorrelationId ?? CorrelationContext.GetOrCreateCorrelationId(),
            Activity.Current?.TraceId.ToString());

    public IntegrationEventEnvelope WithCorrelation() =>
        this with
        {
            CorrelationId = CorrelationId ?? CorrelationContext.CorrelationId ?? CorrelationContext.GetOrCreateCorrelationId(),
            TraceId = TraceId ?? CorrelationContext.TraceId ?? Activity.Current?.TraceId.ToString()
        };
}

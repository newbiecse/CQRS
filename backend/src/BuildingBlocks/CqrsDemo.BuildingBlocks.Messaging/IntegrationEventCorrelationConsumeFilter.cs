using System.Diagnostics;
using CqrsDemo.Contracts.Messaging;
using MassTransit;

namespace CqrsDemo.BuildingBlocks.Messaging;

public sealed class IntegrationEventCorrelationConsumeFilter : IFilter<ConsumeContext<IntegrationEventEnvelope>>
{
    public void Probe(ProbeContext context) => context.CreateFilterScope("integration-event-correlation");

    public async Task Send(
        ConsumeContext<IntegrationEventEnvelope> context,
        IPipe<ConsumeContext<IntegrationEventEnvelope>> next)
    {
        var message = context.Message;
        var correlationId = message.CorrelationId;

        if (string.IsNullOrWhiteSpace(correlationId)
            && context.Headers.TryGetHeader(CorrelationHeaders.CorrelationId, out var headerValue))
        {
            correlationId = headerValue?.ToString();
        }

        correlationId ??= CorrelationContext.GetOrCreateCorrelationId();
        var traceId = message.TraceId ?? Activity.Current?.TraceId.ToString();

        using (CorrelationContext.BeginScope(correlationId, traceId))
        {
            await next.Send(context);
        }
    }
}

using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.Contracts.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CqrsDemo.BuildingBlocks.Messaging;

public sealed class MassTransitKafkaIntegrationEventPublisher(
    ITopicProducer<string, IntegrationEventEnvelope> producer,
    ILogger<MassTransitKafkaIntegrationEventPublisher> logger) : IIntegrationEventPublisher
{
    public async Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        envelope = envelope.WithCorrelation();

        await producer.Produce(
            envelope.EventType,
            envelope,
            Pipe.Execute<SendContext>(context =>
            {
                context.Headers.Set("EventType", envelope.EventType);
                if (!string.IsNullOrWhiteSpace(envelope.CorrelationId))
                    context.Headers.Set(CorrelationHeaders.CorrelationId, envelope.CorrelationId);
                if (!string.IsNullOrWhiteSpace(envelope.TraceId))
                    context.Headers.Set(CorrelationHeaders.TraceId, envelope.TraceId);
            }),
            cancellationToken);

        logger.LogInformation(
            "Published {EventType} to Kafka with CorrelationId {CorrelationId}",
            envelope.EventType,
            envelope.CorrelationId);
    }
}

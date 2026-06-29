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
        await producer.Produce(
            envelope.EventType,
            envelope,
            Pipe.Execute<SendContext>(context => context.Headers.Set("EventType", envelope.EventType)),
            cancellationToken);

        logger.LogInformation("Published {EventType} to Kafka", envelope.EventType);
    }
}

using CqrsDemo.Contracts.Messaging;

namespace CqrsDemo.Messaging.Abstractions;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken = default);
}

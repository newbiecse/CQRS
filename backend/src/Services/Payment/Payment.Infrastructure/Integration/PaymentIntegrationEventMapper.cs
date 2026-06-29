using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Payments;
using CqrsDemo.Contracts.Serialization;
using Payment.Domain.Events;

namespace Payment.Infrastructure.Integration;

public sealed class PaymentIntegrationEventMapper : IIntegrationEventMapper
{
    public IReadOnlyList<OutboxMessageDto> Map(IEnumerable<IDomainEvent> domainEvents) =>
        domainEvents.Select(MapSingle).ToList();

    private static OutboxMessageDto MapSingle(IDomainEvent domainEvent) => domainEvent switch
    {
        PaymentInitiatedEvent e => new(
            IntegrationEventTypes.PaymentInitiated,
            IntegrationEventSerializer.Serialize(new PaymentInitiatedIntegrationEvent(
                e.PaymentId, e.OrderId, e.Amount, e.InitiatedAt))),
        PaymentCompletedEvent e => new(
            IntegrationEventTypes.PaymentCompleted,
            IntegrationEventSerializer.Serialize(new PaymentCompletedIntegrationEvent(
                e.PaymentId, e.OrderId, e.Amount, e.CompletedAt))),
        PaymentFailedEvent e => new(
            IntegrationEventTypes.PaymentFailed,
            IntegrationEventSerializer.Serialize(new PaymentFailedIntegrationEvent(
                e.PaymentId, e.OrderId, e.Reason, e.FailedAt))),
        _ => throw new NotSupportedException($"Domain event {domainEvent.GetType().Name} is not mapped.")
    };
}

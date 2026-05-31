using System.Text.Json;
using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using CqrsDemo.Contracts.Serialization;
using Payment.Domain.Events;

namespace Payment.Infrastructure.EventStore;

public sealed class PaymentDomainEventSerializer : IDomainEventSerializer
{
    private static readonly Dictionary<string, Type> EventTypes = new(StringComparer.Ordinal)
    {
        [nameof(PaymentInitiatedEvent)] = typeof(PaymentInitiatedEvent),
        [nameof(PaymentCompletedEvent)] = typeof(PaymentCompletedEvent),
        [nameof(PaymentFailedEvent)] = typeof(PaymentFailedEvent)
    };

    public string GetTypeName(IDomainEvent domainEvent) => domainEvent.GetType().Name;

    public string Serialize(IDomainEvent domainEvent) =>
        JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), IntegrationEventSerializer.Options);

    public IDomainEvent Deserialize(string eventType, string payload)
    {
        if (!EventTypes.TryGetValue(eventType, out var clrType))
            throw new NotSupportedException($"Unknown stored event type '{eventType}'.");
        return JsonSerializer.Deserialize(payload, clrType, IntegrationEventSerializer.Options) as IDomainEvent
            ?? throw new InvalidOperationException($"Payload is not a valid {eventType}.");
    }
}

using System.Text.Json;
using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using CqrsDemo.Contracts.Serialization;
using Order.Domain.Events;

namespace Order.Infrastructure.EventStore;

public sealed class OrderDomainEventSerializer : IDomainEventSerializer
{
    private static readonly Dictionary<string, Type> EventTypes = new(StringComparer.Ordinal)
    {
        [nameof(OrderCreatedEvent)] = typeof(OrderCreatedEvent),
        [nameof(OrderPaidEvent)] = typeof(OrderPaidEvent),
        [nameof(OrderCancelledEvent)] = typeof(OrderCancelledEvent)
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

using System.Text.Json;
using Cart.Domain.Events;
using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using CqrsDemo.Contracts.Serialization;

namespace Cart.Infrastructure.EventStore;

public sealed class CartDomainEventSerializer : IDomainEventSerializer
{
    private static readonly Dictionary<string, Type> EventTypes = new(StringComparer.Ordinal)
    {
        [nameof(CartCreatedEvent)] = typeof(CartCreatedEvent),
        [nameof(CartItemAddedEvent)] = typeof(CartItemAddedEvent),
        [nameof(CartItemRemovedEvent)] = typeof(CartItemRemovedEvent),
        [nameof(CartCheckedOutEvent)] = typeof(CartCheckedOutEvent)
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

using System.Text.Json;
using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using CqrsDemo.Contracts.Serialization;
using Product.Domain.Events;

namespace Product.Infrastructure.EventStore;

public sealed class ProductDomainEventSerializer : IDomainEventSerializer
{
    private static readonly Dictionary<string, Type> EventTypes = new(StringComparer.Ordinal)
    {
        [nameof(ProductCreatedEvent)] = typeof(ProductCreatedEvent),
        [nameof(ProductPriceUpdatedEvent)] = typeof(ProductPriceUpdatedEvent)
    };

    public string GetTypeName(IDomainEvent domainEvent) => domainEvent.GetType().Name;

    public string Serialize(IDomainEvent domainEvent) =>
        JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), IntegrationEventSerializer.Options);

    public IDomainEvent Deserialize(string eventType, string payload)
    {
        if (!EventTypes.TryGetValue(eventType, out var clrType))
            throw new NotSupportedException($"Unknown event '{eventType}'.");
        return JsonSerializer.Deserialize(payload, clrType, IntegrationEventSerializer.Options) as IDomainEvent
            ?? throw new InvalidOperationException($"Invalid {eventType}.");
    }
}

using System.Text.Json;
using CqrsDemo.Contracts.Serialization;
using CqrsDemo.Domain.Carts.Events;
using CqrsDemo.Domain.Common;
using CqrsDemo.Domain.Orders.Events;
using CqrsDemo.Domain.Payments.Events;
using CqrsDemo.Domain.Products.Events;

namespace CqrsDemo.Commands.Infrastructure.Persistence.EventStore;

public static class DomainEventSerializer
{
    private static readonly Dictionary<string, Type> EventTypes = new(StringComparer.Ordinal)
    {
        [nameof(ProductCreatedEvent)] = typeof(ProductCreatedEvent),
        [nameof(ProductPriceUpdatedEvent)] = typeof(ProductPriceUpdatedEvent),
        [nameof(CartCreatedEvent)] = typeof(CartCreatedEvent),
        [nameof(CartItemAddedEvent)] = typeof(CartItemAddedEvent),
        [nameof(CartItemRemovedEvent)] = typeof(CartItemRemovedEvent),
        [nameof(CartCheckedOutEvent)] = typeof(CartCheckedOutEvent),
        [nameof(OrderCreatedEvent)] = typeof(OrderCreatedEvent),
        [nameof(OrderPaidEvent)] = typeof(OrderPaidEvent),
        [nameof(PaymentInitiatedEvent)] = typeof(PaymentInitiatedEvent),
        [nameof(PaymentCompletedEvent)] = typeof(PaymentCompletedEvent),
        [nameof(PaymentFailedEvent)] = typeof(PaymentFailedEvent)
    };

    public static string GetTypeName(IDomainEvent domainEvent) => domainEvent.GetType().Name;

    public static string Serialize(IDomainEvent domainEvent) =>
        JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), IntegrationEventSerializer.Options);

    public static IDomainEvent Deserialize(string eventType, string payload)
    {
        if (!EventTypes.TryGetValue(eventType, out var clrType))
        {
            throw new NotSupportedException($"Unknown stored event type '{eventType}'.");
        }

        var domainEvent = JsonSerializer.Deserialize(payload, clrType, IntegrationEventSerializer.Options);
        return domainEvent as IDomainEvent
            ?? throw new InvalidOperationException($"Payload is not a valid {eventType}.");
    }
}

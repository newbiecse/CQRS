using Cart.Domain.Events;
using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using CqrsDemo.Contracts.Carts;
using CqrsDemo.Contracts.Common;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Serialization;

namespace Cart.Infrastructure.EventStore;

public sealed class CartIntegrationEventMapper : IIntegrationEventMapper
{
    public IReadOnlyList<OutboxMessageDto> Map(IEnumerable<IDomainEvent> domainEvents) =>
        domainEvents.Select(MapSingle).ToList();

    private static OutboxMessageDto MapSingle(IDomainEvent domainEvent) => domainEvent switch
    {
        CartCreatedEvent e => new(
            IntegrationEventTypes.CartCreated,
            IntegrationEventSerializer.Serialize(new CartCreatedIntegrationEvent(e.CartId, e.CustomerId, e.CreatedAt))),
        CartItemAddedEvent e => new(
            IntegrationEventTypes.CartItemAdded,
            IntegrationEventSerializer.Serialize(new CartItemAddedIntegrationEvent(
                e.CartId, e.ProductId, e.ProductName, e.UnitPrice, e.Quantity))),
        CartItemRemovedEvent e => new(
            IntegrationEventTypes.CartItemRemoved,
            IntegrationEventSerializer.Serialize(new CartItemRemovedIntegrationEvent(e.CartId, e.ProductId))),
        CartCheckedOutEvent e => new(
            IntegrationEventTypes.CartCheckedOut,
            IntegrationEventSerializer.Serialize(new CartCheckedOutIntegrationEvent(
                e.CartId,
                e.OrderId,
                e.CustomerId,
                e.Lines.Select(ToDto).ToList(),
                e.TotalAmount,
                e.CheckedOutAt))),
        _ => throw new NotSupportedException($"Domain event {domainEvent.GetType().Name} is not mapped.")
    };

    private static OrderLineDto ToDto(OrderLine line) =>
        new(line.ProductId, line.ProductName, line.UnitPrice, line.Quantity);
}

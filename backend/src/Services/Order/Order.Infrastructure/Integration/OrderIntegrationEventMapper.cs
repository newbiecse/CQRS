using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.Contracts.Common;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Orders;
using CqrsDemo.Contracts.Serialization;
using Order.Domain.Events;

namespace Order.Infrastructure.Integration;

public sealed class OrderIntegrationEventMapper : IIntegrationEventMapper
{
    public IReadOnlyList<OutboxMessageDto> Map(IEnumerable<IDomainEvent> domainEvents) =>
        domainEvents.Select(MapSingle).ToList();

    private static OutboxMessageDto MapSingle(IDomainEvent domainEvent) => domainEvent switch
    {
        OrderCreatedEvent e => new(
            IntegrationEventTypes.OrderCreated,
            IntegrationEventSerializer.Serialize(new OrderCreatedIntegrationEvent(
                e.OrderId, e.CustomerId, e.CartId, e.Lines.Select(ToDto).ToList(), e.TotalAmount, e.CreatedAt))),
        OrderPaidEvent e => new(
            IntegrationEventTypes.OrderPaid,
            IntegrationEventSerializer.Serialize(new OrderPaidIntegrationEvent(
                e.OrderId, e.PaymentId, e.Amount, e.PaidAt))),
        OrderUpdatedEvent e => new(
            IntegrationEventTypes.OrderUpdated,
            IntegrationEventSerializer.Serialize(new OrderUpdatedIntegrationEvent(
                e.OrderId, e.CustomerId, e.CartId, e.Lines.Select(ToDto).ToList(), e.TotalAmount, e.UpdatedAt))),
        OrderCancelledEvent e => new(
            IntegrationEventTypes.OrderCancelled,
            IntegrationEventSerializer.Serialize(new OrderCancelledIntegrationEvent(
                e.OrderId, e.CartId, e.Reason, e.CancelledAt))),
        _ => throw new NotSupportedException($"Domain event {domainEvent.GetType().Name} is not mapped.")
    };

    private static OrderLineDto ToDto(OrderLine line) =>
        new(line.ProductId, line.ProductName, line.UnitPrice, line.Quantity);
}

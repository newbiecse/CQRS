using CqrsDemo.Contracts.Carts;
using CqrsDemo.Contracts.Common;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Orders;
using CqrsDemo.Contracts.Payments;
using CqrsDemo.Contracts.Products;
using CqrsDemo.Contracts.Serialization;
using CqrsDemo.Domain.Carts.Events;
using CqrsDemo.Domain.Common;
using CqrsDemo.Domain.Orders.Events;
using CqrsDemo.Domain.Payments.Events;
using CqrsDemo.Domain.Products.Events;

namespace CqrsDemo.Commands.Application.Integration;

public static class DomainEventToOutboxMapper
{
    public static IReadOnlyList<OutboxMessageDto> Map(IEnumerable<IDomainEvent> domainEvents)
    {
        var messages = new List<OutboxMessageDto>();

        foreach (var domainEvent in domainEvents)
        {
            messages.Add(MapSingle(domainEvent));
        }

        return messages;
    }

    private static OutboxMessageDto MapSingle(IDomainEvent domainEvent) =>
        domainEvent switch
        {
            ProductCreatedEvent created => new(
                IntegrationEventTypes.ProductCreated,
                IntegrationEventSerializer.Serialize(new ProductCreatedIntegrationEvent(
                    created.ProductId, created.Name, created.Price, created.CreatedAt))),

            ProductPriceUpdatedEvent updated => new(
                IntegrationEventTypes.ProductPriceUpdated,
                IntegrationEventSerializer.Serialize(new ProductPriceUpdatedIntegrationEvent(
                    updated.ProductId, updated.OldPrice, updated.NewPrice, updated.UpdatedAt))),

            CartCreatedEvent cartCreated => new(
                IntegrationEventTypes.CartCreated,
                IntegrationEventSerializer.Serialize(new CartCreatedIntegrationEvent(
                    cartCreated.CartId, cartCreated.CustomerId, cartCreated.CreatedAt))),

            CartItemAddedEvent itemAdded => new(
                IntegrationEventTypes.CartItemAdded,
                IntegrationEventSerializer.Serialize(new CartItemAddedIntegrationEvent(
                    itemAdded.CartId, itemAdded.ProductId, itemAdded.ProductName,
                    itemAdded.UnitPrice, itemAdded.Quantity))),

            CartItemRemovedEvent itemRemoved => new(
                IntegrationEventTypes.CartItemRemoved,
                IntegrationEventSerializer.Serialize(new CartItemRemovedIntegrationEvent(
                    itemRemoved.CartId, itemRemoved.ProductId))),

            CartCheckedOutEvent checkedOut => new(
                IntegrationEventTypes.CartCheckedOut,
                IntegrationEventSerializer.Serialize(new CartCheckedOutIntegrationEvent(
                    checkedOut.CartId,
                    checkedOut.OrderId,
                    checkedOut.CustomerId,
                    checkedOut.Lines.Select(ToDto).ToList(),
                    checkedOut.TotalAmount,
                    checkedOut.CheckedOutAt))),

            OrderCreatedEvent orderCreated => new(
                IntegrationEventTypes.OrderCreated,
                IntegrationEventSerializer.Serialize(new OrderCreatedIntegrationEvent(
                    orderCreated.OrderId,
                    orderCreated.CustomerId,
                    orderCreated.CartId,
                    orderCreated.Lines.Select(ToDto).ToList(),
                    orderCreated.TotalAmount,
                    orderCreated.CreatedAt))),

            OrderPaidEvent orderPaid => new(
                IntegrationEventTypes.OrderPaid,
                IntegrationEventSerializer.Serialize(new OrderPaidIntegrationEvent(
                    orderPaid.OrderId, orderPaid.PaymentId, orderPaid.Amount, orderPaid.PaidAt))),

            PaymentInitiatedEvent paymentInitiated => new(
                IntegrationEventTypes.PaymentInitiated,
                IntegrationEventSerializer.Serialize(new PaymentInitiatedIntegrationEvent(
                    paymentInitiated.PaymentId, paymentInitiated.OrderId,
                    paymentInitiated.Amount, paymentInitiated.InitiatedAt))),

            PaymentCompletedEvent paymentCompleted => new(
                IntegrationEventTypes.PaymentCompleted,
                IntegrationEventSerializer.Serialize(new PaymentCompletedIntegrationEvent(
                    paymentCompleted.PaymentId, paymentCompleted.OrderId,
                    paymentCompleted.Amount, paymentCompleted.CompletedAt))),

            PaymentFailedEvent paymentFailed => new(
                IntegrationEventTypes.PaymentFailed,
                IntegrationEventSerializer.Serialize(new PaymentFailedIntegrationEvent(
                    paymentFailed.PaymentId, paymentFailed.OrderId,
                    paymentFailed.Reason, paymentFailed.FailedAt))),

            _ => throw new NotSupportedException(
                $"Domain event {domainEvent.GetType().Name} is not mapped to an integration event.")
        };

    private static OrderLineDto ToDto(OrderLine line) =>
        new(line.ProductId, line.ProductName, line.UnitPrice, line.Quantity);
}

public sealed record OutboxMessageDto(string EventType, string Payload);

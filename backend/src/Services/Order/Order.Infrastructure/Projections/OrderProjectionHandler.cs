using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Orders;
using CqrsDemo.Contracts.Serialization;
using Microsoft.Extensions.Logging;
using Order.Application.Abstractions;
using Order.Application.ReadModels;

namespace Order.Infrastructure.Projections;

public sealed class OrderProjectionHandler(IOrderReadRepository repository, ILogger<OrderProjectionHandler> logger)
{
    public async Task HandleAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        switch (eventType)
        {
            case IntegrationEventTypes.OrderCreated:
                await ProjectOrderCreatedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.OrderPaid:
                await ProjectOrderPaidAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.OrderCancelled:
                await ProjectOrderCancelledAsync(payload, cancellationToken);
                break;
            default:
                logger.LogWarning("Unknown integration event type: {EventType}", eventType);
                break;
        }
    }

    private async Task ProjectOrderCreatedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<OrderCreatedIntegrationEvent>(payload);
        await repository.UpsertAsync(new OrderReadModel
        {
            Id = e.OrderId,
            CustomerId = e.CustomerId,
            CartId = e.CartId,
            Status = "PendingPayment",
            Lines = e.Lines,
            TotalAmount = e.TotalAmount,
            CreatedAt = e.CreatedAt,
            LastUpdatedAt = e.CreatedAt
        }, cancellationToken);
    }

    private async Task ProjectOrderPaidAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<OrderPaidIntegrationEvent>(payload);
        var order = await repository.GetByIdAsync(e.OrderId, cancellationToken);
        if (order is null) return;

        await repository.UpsertAsync(new OrderReadModel
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CartId = order.CartId,
            Status = "Paid",
            Lines = order.Lines,
            TotalAmount = order.TotalAmount,
            PaymentId = e.PaymentId,
            CreatedAt = order.CreatedAt,
            LastUpdatedAt = e.PaidAt
        }, cancellationToken);
    }

    private async Task ProjectOrderCancelledAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<OrderCancelledIntegrationEvent>(payload);
        var order = await repository.GetByIdAsync(e.OrderId, cancellationToken);
        if (order is null) return;

        await repository.UpsertAsync(new OrderReadModel
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CartId = order.CartId,
            Status = "Cancelled",
            Lines = order.Lines,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            LastUpdatedAt = e.CancelledAt
        }, cancellationToken);
    }
}

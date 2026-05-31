using CqrsDemo.Contracts.Carts;
using CqrsDemo.Contracts.Common;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Orders;
using CqrsDemo.Contracts.Payments;
using CqrsDemo.Contracts.Products;
using CqrsDemo.Contracts.Serialization;
using CqrsDemo.Queries.Application.Abstractions;
using CqrsDemo.Queries.Application.Carts.ReadModels;
using CqrsDemo.Queries.Application.Orders.ReadModels;
using CqrsDemo.Queries.Application.Payments.ReadModels;
using CqrsDemo.Queries.Application.Products.ReadModels;
using Microsoft.Extensions.Logging;

namespace CqrsDemo.Projection.Worker.Projections;

public sealed class ShopProjectionHandler(
    IProductReadRepository productRepository,
    ICartReadRepository cartRepository,
    IOrderReadRepository orderRepository,
    IPaymentReadRepository paymentRepository,
    ILogger<ShopProjectionHandler> logger)
{
    public async Task HandleAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        switch (eventType)
        {
            case IntegrationEventTypes.ProductCreated:
                await ProjectProductCreatedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.ProductPriceUpdated:
                await ProjectProductPriceUpdatedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.CartCreated:
                await ProjectCartCreatedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.CartItemAdded:
                await ProjectCartItemAddedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.CartItemRemoved:
                await ProjectCartItemRemovedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.CartCheckedOut:
                await ProjectCartCheckedOutAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.OrderCreated:
                await ProjectOrderCreatedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.OrderPaid:
                await ProjectOrderPaidAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.PaymentInitiated:
                await ProjectPaymentInitiatedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.PaymentCompleted:
                await ProjectPaymentCompletedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.PaymentFailed:
                await ProjectPaymentFailedAsync(payload, cancellationToken);
                break;
            default:
                logger.LogWarning("Unknown integration event type: {EventType}", eventType);
                break;
        }
    }

    private async Task ProjectProductCreatedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<ProductCreatedIntegrationEvent>(payload);
        await productRepository.UpsertAsync(new ProductReadModel
        {
            Id = e.ProductId,
            Name = e.Name,
            Price = e.Price,
            CreatedAt = e.CreatedAt,
            LastUpdatedAt = e.CreatedAt
        }, cancellationToken);
    }

    private async Task ProjectProductPriceUpdatedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<ProductPriceUpdatedIntegrationEvent>(payload);
        var existing = await productRepository.GetByIdAsync(e.ProductId, cancellationToken);
        if (existing is null) return;

        await productRepository.UpsertAsync(new ProductReadModel
        {
            Id = existing.Id,
            Name = existing.Name,
            Price = e.NewPrice,
            CreatedAt = existing.CreatedAt,
            LastUpdatedAt = e.UpdatedAt
        }, cancellationToken);
    }

    private async Task ProjectCartCreatedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<CartCreatedIntegrationEvent>(payload);
        await cartRepository.UpsertAsync(new CartReadModel
        {
            Id = e.CartId,
            CustomerId = e.CustomerId,
            Status = "Active",
            Items = [],
            Subtotal = 0,
            CreatedAt = e.CreatedAt,
            LastUpdatedAt = e.CreatedAt
        }, cancellationToken);
    }

    private async Task ProjectCartItemAddedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<CartItemAddedIntegrationEvent>(payload);
        var cart = await cartRepository.GetByIdAsync(e.CartId, cancellationToken);
        if (cart is null) return;

        var items = cart.Items.ToList();
        var existing = items.FirstOrDefault(i => i.ProductId == e.ProductId);
        if (existing is null)
        {
            items.Add(new OrderLineDto(e.ProductId, e.ProductName, e.UnitPrice, e.Quantity));
        }
        else
        {
            items.Remove(existing);
            items.Add(new OrderLineDto(
                e.ProductId, e.ProductName, e.UnitPrice, existing.Quantity + e.Quantity));
        }

        await UpsertCartAsync(cart, items, cancellationToken);
    }

    private async Task ProjectCartItemRemovedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<CartItemRemovedIntegrationEvent>(payload);
        var cart = await cartRepository.GetByIdAsync(e.CartId, cancellationToken);
        if (cart is null) return;

        var items = cart.Items.Where(i => i.ProductId != e.ProductId).ToList();
        await UpsertCartAsync(cart, items, cancellationToken);
    }

    private async Task ProjectCartCheckedOutAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<CartCheckedOutIntegrationEvent>(payload);
        var cart = await cartRepository.GetByIdAsync(e.CartId, cancellationToken);
        if (cart is null) return;

        await cartRepository.UpsertAsync(new CartReadModel
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            Status = "CheckedOut",
            Items = cart.Items,
            Subtotal = cart.Subtotal,
            CreatedAt = cart.CreatedAt,
            LastUpdatedAt = e.CheckedOutAt
        }, cancellationToken);
    }

    private async Task UpsertCartAsync(
        CartReadModel cart,
        List<OrderLineDto> items,
        CancellationToken cancellationToken)
    {
        await cartRepository.UpsertAsync(new CartReadModel
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            Status = cart.Status,
            Items = items,
            Subtotal = items.Sum(i => i.LineTotal),
            CreatedAt = cart.CreatedAt,
            LastUpdatedAt = DateTime.UtcNow
        }, cancellationToken);
    }

    private async Task ProjectOrderCreatedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<OrderCreatedIntegrationEvent>(payload);
        await orderRepository.UpsertAsync(new OrderReadModel
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
        var order = await orderRepository.GetByIdAsync(e.OrderId, cancellationToken);
        if (order is null) return;

        await orderRepository.UpsertAsync(new OrderReadModel
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

    private async Task ProjectPaymentInitiatedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<PaymentInitiatedIntegrationEvent>(payload);
        await paymentRepository.UpsertAsync(new PaymentReadModel
        {
            Id = e.PaymentId,
            OrderId = e.OrderId,
            Amount = e.Amount,
            Status = "Pending",
            InitiatedAt = e.InitiatedAt,
            LastUpdatedAt = e.InitiatedAt
        }, cancellationToken);
    }

    private async Task ProjectPaymentCompletedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<PaymentCompletedIntegrationEvent>(payload);
        var existing = await paymentRepository.GetByIdAsync(e.PaymentId, cancellationToken);
        await paymentRepository.UpsertAsync(new PaymentReadModel
        {
            Id = e.PaymentId,
            OrderId = e.OrderId,
            Amount = e.Amount,
            Status = "Completed",
            FailureReason = null,
            InitiatedAt = existing?.InitiatedAt ?? e.CompletedAt,
            LastUpdatedAt = e.CompletedAt
        }, cancellationToken);
    }

    private async Task ProjectPaymentFailedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<PaymentFailedIntegrationEvent>(payload);
        var existing = await paymentRepository.GetByIdAsync(e.PaymentId, cancellationToken);
        await paymentRepository.UpsertAsync(new PaymentReadModel
        {
            Id = e.PaymentId,
            OrderId = e.OrderId,
            Amount = existing?.Amount ?? 0,
            Status = "Failed",
            FailureReason = e.Reason,
            InitiatedAt = existing?.InitiatedAt ?? e.FailedAt,
            LastUpdatedAt = e.FailedAt
        }, cancellationToken);
    }
}

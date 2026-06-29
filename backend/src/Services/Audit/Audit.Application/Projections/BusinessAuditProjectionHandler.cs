using Audit.Application.Abstractions;
using Audit.Application.Models;
using CqrsDemo.Contracts.Carts;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Orders;
using CqrsDemo.Contracts.Payments;
using CqrsDemo.Contracts.Products;
using CqrsDemo.Contracts.Saga;
using CqrsDemo.Contracts.Serialization;
using CqrsDemo.Contracts.Users;

namespace Audit.Application.Projections;

public sealed class BusinessAuditProjectionHandler(IBusinessAuditWriter writer)
{
    public async Task HandleAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        var record = MapToAuditRecord(eventType, payload);
        if (record is null)
            return;

        await writer.IndexAsync(record, cancellationToken);
    }

    private static BusinessAuditRecord? MapToAuditRecord(string eventType, string payload) =>
        eventType switch
        {
            IntegrationEventTypes.ProductCreated => MapProductCreated(payload),
            IntegrationEventTypes.ProductPriceUpdated => MapProductPriceUpdated(payload),
            IntegrationEventTypes.UserRegistered => MapUserRegistered(payload),
            IntegrationEventTypes.UserProfileUpdated => MapUserProfileUpdated(payload),
            IntegrationEventTypes.UserDeactivated => MapUserDeactivated(payload),
            IntegrationEventTypes.CartCreated => MapCartCreated(payload),
            IntegrationEventTypes.CartItemAdded => MapCartItemAdded(payload),
            IntegrationEventTypes.CartItemRemoved => MapCartItemRemoved(payload),
            IntegrationEventTypes.CartCheckedOut => MapCartCheckedOut(payload),
            IntegrationEventTypes.OrderCreated => MapOrderCreated(payload),
            IntegrationEventTypes.OrderPaid => MapOrderPaid(payload),
            IntegrationEventTypes.OrderCancelled => MapOrderCancelled(payload),
            IntegrationEventTypes.PaymentInitiated => MapPaymentInitiated(payload),
            IntegrationEventTypes.PaymentCompleted => MapPaymentCompleted(payload),
            IntegrationEventTypes.PaymentFailed => MapPaymentFailed(payload),
            IntegrationEventTypes.CheckoutSagaCompleted => MapCheckoutSagaCompleted(payload),
            IntegrationEventTypes.CheckoutSagaFailed => MapCheckoutSagaFailed(payload),
            _ => null
        };

    private static BusinessAuditRecord MapProductCreated(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<ProductCreatedIntegrationEvent>(payload);
        return Create(
            e.CreatedAt,
            IntegrationEventTypes.ProductCreated,
            "Product",
            e.ProductId.ToString(),
            "Created",
            "Product",
            $"Product '{e.Name}' created at price {e.Price:F2}",
            payload);
    }

    private static BusinessAuditRecord MapProductPriceUpdated(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<ProductPriceUpdatedIntegrationEvent>(payload);
        return Create(
            e.UpdatedAt,
            IntegrationEventTypes.ProductPriceUpdated,
            "Product",
            e.ProductId.ToString(),
            "PriceUpdated",
            "Product",
            $"Product price changed from {e.OldPrice:F2} to {e.NewPrice:F2}",
            payload);
    }

    private static BusinessAuditRecord MapUserRegistered(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<UserRegisteredIntegrationEvent>(payload);
        return Create(
            e.RegisteredAt,
            IntegrationEventTypes.UserRegistered,
            "User",
            e.UserId.ToString(),
            "Registered",
            "User",
            $"User '{e.DisplayName}' registered ({e.Email})",
            payload);
    }

    private static BusinessAuditRecord MapUserProfileUpdated(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<UserProfileUpdatedIntegrationEvent>(payload);
        return Create(
            e.UpdatedAt,
            IntegrationEventTypes.UserProfileUpdated,
            "User",
            e.UserId.ToString(),
            "ProfileUpdated",
            "User",
            $"User profile updated to '{e.DisplayName}'",
            payload);
    }

    private static BusinessAuditRecord MapUserDeactivated(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<UserDeactivatedIntegrationEvent>(payload);
        return Create(
            e.DeactivatedAt,
            IntegrationEventTypes.UserDeactivated,
            "User",
            e.UserId.ToString(),
            "Deactivated",
            "User",
            "User deactivated",
            payload);
    }

    private static BusinessAuditRecord MapCartCreated(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<CartCreatedIntegrationEvent>(payload);
        return Create(
            e.CreatedAt,
            IntegrationEventTypes.CartCreated,
            "Cart",
            e.CartId.ToString(),
            "Created",
            "Cart",
            $"Cart created for customer {e.CustomerId}",
            payload);
    }

    private static BusinessAuditRecord MapCartItemAdded(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<CartItemAddedIntegrationEvent>(payload);
        return Create(
            DateTime.UtcNow,
            IntegrationEventTypes.CartItemAdded,
            "Cart",
            e.CartId.ToString(),
            "ItemAdded",
            "Cart",
            $"Added {e.Quantity}x '{e.ProductName}' to cart",
            payload);
    }

    private static BusinessAuditRecord MapCartItemRemoved(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<CartItemRemovedIntegrationEvent>(payload);
        return Create(
            DateTime.UtcNow,
            IntegrationEventTypes.CartItemRemoved,
            "Cart",
            e.CartId.ToString(),
            "ItemRemoved",
            "Cart",
            $"Removed product {e.ProductId} from cart",
            payload);
    }

    private static BusinessAuditRecord MapCartCheckedOut(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<CartCheckedOutIntegrationEvent>(payload);
        return Create(
            e.CheckedOutAt,
            IntegrationEventTypes.CartCheckedOut,
            "Cart",
            e.CartId.ToString(),
            "CheckedOut",
            "Cart",
            $"Cart checked out, order {e.OrderId}",
            payload);
    }

    private static BusinessAuditRecord MapOrderCreated(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<OrderCreatedIntegrationEvent>(payload);
        return Create(
            e.CreatedAt,
            IntegrationEventTypes.OrderCreated,
            "Order",
            e.OrderId.ToString(),
            "Created",
            "Order",
            $"Order created for customer {e.CustomerId}, total {e.TotalAmount:F2}",
            payload);
    }

    private static BusinessAuditRecord MapOrderPaid(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<OrderPaidIntegrationEvent>(payload);
        return Create(
            e.PaidAt,
            IntegrationEventTypes.OrderPaid,
            "Order",
            e.OrderId.ToString(),
            "Paid",
            "Order",
            $"Order paid, amount {e.Amount:F2}",
            payload);
    }

    private static BusinessAuditRecord MapOrderCancelled(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<OrderCancelledIntegrationEvent>(payload);
        return Create(
            e.CancelledAt,
            IntegrationEventTypes.OrderCancelled,
            "Order",
            e.OrderId.ToString(),
            "Cancelled",
            "Order",
            $"Order cancelled: {e.Reason}",
            payload);
    }

    private static BusinessAuditRecord MapPaymentInitiated(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<PaymentInitiatedIntegrationEvent>(payload);
        return Create(
            e.InitiatedAt,
            IntegrationEventTypes.PaymentInitiated,
            "Payment",
            e.PaymentId.ToString(),
            "Initiated",
            "Payment",
            $"Payment initiated for order {e.OrderId}, amount {e.Amount:F2}",
            payload);
    }

    private static BusinessAuditRecord MapPaymentCompleted(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<PaymentCompletedIntegrationEvent>(payload);
        return Create(
            e.CompletedAt,
            IntegrationEventTypes.PaymentCompleted,
            "Payment",
            e.PaymentId.ToString(),
            "Completed",
            "Payment",
            $"Payment completed for order {e.OrderId}",
            payload);
    }

    private static BusinessAuditRecord MapPaymentFailed(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<PaymentFailedIntegrationEvent>(payload);
        return Create(
            e.FailedAt,
            IntegrationEventTypes.PaymentFailed,
            "Payment",
            e.PaymentId.ToString(),
            "Failed",
            "Payment",
            $"Payment failed for order {e.OrderId}: {e.Reason}",
            payload);
    }

    private static BusinessAuditRecord MapCheckoutSagaCompleted(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<CheckoutSagaCompletedIntegrationEvent>(payload);
        return Create(
            e.CompletedAt,
            IntegrationEventTypes.CheckoutSagaCompleted,
            "CheckoutSaga",
            e.SagaId.ToString(),
            "Completed",
            "Saga",
            $"Checkout saga completed for order {e.OrderId}",
            payload);
    }

    private static BusinessAuditRecord MapCheckoutSagaFailed(string payload)
    {
        var e = IntegrationEventSerializer.Deserialize<CheckoutSagaFailedIntegrationEvent>(payload);
        return Create(
            e.FailedAt,
            IntegrationEventTypes.CheckoutSagaFailed,
            "CheckoutSaga",
            e.SagaId.ToString(),
            "Failed",
            "Saga",
            $"Checkout saga failed: {e.Reason}",
            payload);
    }

    private static BusinessAuditRecord Create(
        DateTime occurredAtUtc,
        string eventType,
        string entityType,
        string entityId,
        string action,
        string service,
        string summary,
        string payloadJson) =>
        new()
        {
            OccurredAtUtc = occurredAtUtc,
            EventType = eventType,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Service = service,
            Summary = summary,
            PayloadJson = payloadJson
        };
}

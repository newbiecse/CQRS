namespace CqrsDemo.Contracts.Messaging;

public static class IntegrationEventTypes
{
    public const string UserRegistered = "user.registered.v1";
    public const string UserProfileUpdated = "user.profile-updated.v1";
    public const string UserDeactivated = "user.deactivated.v1";

    public const string ProductCreated = "product.created.v1";
    public const string ProductPriceUpdated = "product.price-updated.v1";
    public const string ProductUpdated = "product.updated.v1";
    public const string ProductDeleted = "product.deleted.v1";

    public const string CartCreated = "cart.created.v1";
    public const string CartItemAdded = "cart.item-added.v1";
    public const string CartItemRemoved = "cart.item-removed.v1";
    public const string CartCheckedOut = "cart.checked-out.v1";

    public const string OrderCreated = "order.created.v1";
    public const string OrderUpdated = "order.updated.v1";
    public const string OrderPaid = "order.paid.v1";
    public const string OrderCancelled = "order.cancelled.v1";

    public const string PaymentInitiated = "payment.initiated.v1";
    public const string PaymentCompleted = "payment.completed.v1";
    public const string PaymentFailed = "payment.failed.v1";

    public const string CheckoutSagaCompleted = "checkout-saga.completed.v1";
    public const string CheckoutSagaFailed = "checkout-saga.failed.v1";
}

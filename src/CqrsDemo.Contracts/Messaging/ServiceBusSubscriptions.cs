namespace CqrsDemo.Contracts.Messaging;

public static class ServiceBusSubscriptions
{
    public const string UserProjection = "user-projection";
    public const string ReportingProjection = "reporting-projection";
    public const string ProductProjection = "product-projection";
    public const string CartProjection = "cart-projection";
    public const string OrderProjection = "order-projection";
    public const string OrderIntegration = "order-integration";
    public const string PaymentProjection = "payment-projection";
    public const string CheckoutSagaOrchestration = "checkout-saga-orchestration";
}

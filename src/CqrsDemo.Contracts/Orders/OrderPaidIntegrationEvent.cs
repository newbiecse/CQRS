namespace CqrsDemo.Contracts.Orders;

public sealed record OrderPaidIntegrationEvent(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    DateTime PaidAt);

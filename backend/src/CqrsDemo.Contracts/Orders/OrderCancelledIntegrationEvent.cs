namespace CqrsDemo.Contracts.Orders;

public sealed record OrderCancelledIntegrationEvent(
    Guid OrderId,
    Guid CartId,
    string Reason,
    DateTime CancelledAt);

namespace CqrsDemo.Contracts.Payments;

public sealed record PaymentFailedIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    string Reason,
    DateTime FailedAt);

namespace CqrsDemo.Contracts.Payments;

public sealed record PaymentCompletedIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    DateTime CompletedAt);

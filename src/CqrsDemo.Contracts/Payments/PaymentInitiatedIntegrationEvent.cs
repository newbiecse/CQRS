namespace CqrsDemo.Contracts.Payments;

public sealed record PaymentInitiatedIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    DateTime InitiatedAt);

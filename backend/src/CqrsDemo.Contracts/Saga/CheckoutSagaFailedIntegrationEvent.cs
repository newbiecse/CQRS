namespace CqrsDemo.Contracts.Saga;

public sealed record CheckoutSagaFailedIntegrationEvent(
    Guid SagaId,
    Guid CartId,
    Guid? OrderId,
    string Reason,
    DateTime FailedAt);

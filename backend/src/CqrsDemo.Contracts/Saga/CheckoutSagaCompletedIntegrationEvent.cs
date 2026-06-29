namespace CqrsDemo.Contracts.Saga;

public sealed record CheckoutSagaCompletedIntegrationEvent(
    Guid SagaId,
    Guid CartId,
    Guid OrderId,
    Guid PaymentId,
    DateTime CompletedAt);

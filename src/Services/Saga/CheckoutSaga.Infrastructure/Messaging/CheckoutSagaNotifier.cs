using CheckoutSaga.Application.Abstractions;
using CheckoutSaga.Domain;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Saga;
using CqrsDemo.Contracts.Serialization;

namespace CheckoutSaga.Infrastructure.Messaging;

public sealed class CheckoutSagaNotifier(IIntegrationEventPublisher publisher) : ICheckoutSagaNotifier
{
    public Task PublishCompletedAsync(CheckoutSagaInstance saga, CancellationToken cancellationToken)
    {
        if (saga.OrderId is null || saga.PaymentId is null) return Task.CompletedTask;

        var payload = IntegrationEventSerializer.Serialize(new CheckoutSagaCompletedIntegrationEvent(
            saga.Id,
            saga.CartId,
            saga.OrderId.Value,
            saga.PaymentId.Value,
            DateTime.UtcNow));

        return publisher.PublishAsync(
            new IntegrationEventEnvelope(IntegrationEventTypes.CheckoutSagaCompleted, payload),
            cancellationToken);
    }

    public Task PublishFailedAsync(CheckoutSagaInstance saga, CancellationToken cancellationToken)
    {
        var payload = IntegrationEventSerializer.Serialize(new CheckoutSagaFailedIntegrationEvent(
            saga.Id,
            saga.CartId,
            saga.OrderId,
            saga.FailureReason ?? "Checkout saga failed.",
            DateTime.UtcNow));

        return publisher.PublishAsync(
            new IntegrationEventEnvelope(IntegrationEventTypes.CheckoutSagaFailed, payload),
            cancellationToken);
    }
}

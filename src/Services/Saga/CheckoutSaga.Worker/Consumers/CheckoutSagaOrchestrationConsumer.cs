using CheckoutSaga.Application.Orchestration;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Orders;
using CqrsDemo.Contracts.Payments;
using CqrsDemo.Contracts.Serialization;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace CheckoutSaga.Worker.Consumers;

public sealed class CheckoutSagaOrchestrationConsumer(IServiceScopeFactory scopeFactory) : IConsumer<IntegrationEventEnvelope>
{
    public async Task Consume(ConsumeContext<IntegrationEventEnvelope> context)
    {
        using var scope = scopeFactory.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<CheckoutSagaOrchestrator>();
        var message = context.Message;

        switch (message.EventType)
        {
            case IntegrationEventTypes.OrderCreated:
            {
                var e = IntegrationEventSerializer.Deserialize<OrderCreatedIntegrationEvent>(message.Payload);
                await orchestrator.HandleOrderCreatedAsync(e.OrderId, e.CartId, context.CancellationToken);
                break;
            }
            case IntegrationEventTypes.PaymentCompleted:
            {
                var e = IntegrationEventSerializer.Deserialize<PaymentCompletedIntegrationEvent>(message.Payload);
                await orchestrator.HandlePaymentCompletedAsync(e.PaymentId, e.OrderId, e.Amount, context.CancellationToken);
                break;
            }
            case IntegrationEventTypes.PaymentFailed:
            {
                var e = IntegrationEventSerializer.Deserialize<PaymentFailedIntegrationEvent>(message.Payload);
                await orchestrator.HandlePaymentFailedAsync(e.OrderId, e.Reason, context.CancellationToken);
                break;
            }
        }
    }
}

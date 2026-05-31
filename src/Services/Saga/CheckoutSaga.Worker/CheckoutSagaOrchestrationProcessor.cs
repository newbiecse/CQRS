using CheckoutSaga.Application.Orchestration;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Options;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Orders;
using CqrsDemo.Contracts.Payments;
using CqrsDemo.Contracts.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CheckoutSaga.Worker;

public sealed class CheckoutSagaOrchestrationProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<AzureServiceBusOptions> options,
    ILogger<CheckoutSagaOrchestrationProcessor> logger)
    : ServiceBusConsumerBackgroundService(options, logger)
{
    protected override string SubscriptionName => ServiceBusSubscriptions.CheckoutSagaOrchestration;

    protected override async Task ProcessMessageAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<CheckoutSagaOrchestrator>();

        switch (eventType)
        {
            case IntegrationEventTypes.OrderCreated:
            {
                var e = IntegrationEventSerializer.Deserialize<OrderCreatedIntegrationEvent>(payload);
                await orchestrator.HandleOrderCreatedAsync(e.OrderId, e.CartId, cancellationToken);
                break;
            }
            case IntegrationEventTypes.PaymentCompleted:
            {
                var e = IntegrationEventSerializer.Deserialize<PaymentCompletedIntegrationEvent>(payload);
                await orchestrator.HandlePaymentCompletedAsync(e.PaymentId, e.OrderId, e.Amount, cancellationToken);
                break;
            }
            case IntegrationEventTypes.PaymentFailed:
            {
                var e = IntegrationEventSerializer.Deserialize<PaymentFailedIntegrationEvent>(payload);
                await orchestrator.HandlePaymentFailedAsync(e.OrderId, e.Reason, cancellationToken);
                break;
            }
            default:
                logger.LogDebug("Ignoring event type {EventType} on saga subscription", eventType);
                break;
        }
    }
}

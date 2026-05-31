using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Options;
using CqrsDemo.Contracts.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Order.Application.Integration;

namespace Order.Integration.Worker;

public sealed class OrderIntegrationProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<AzureServiceBusOptions> options,
    ILogger<OrderIntegrationProcessor> logger)
    : ServiceBusConsumerBackgroundService(options, logger)
{
    protected override string SubscriptionName => ServiceBusSubscriptions.OrderIntegration;

    protected override async Task ProcessMessageAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetRequiredService<OrderIntegrationHandlers>();
        switch (eventType)
        {
            case IntegrationEventTypes.CartCheckedOut:
                await handlers.HandleCartCheckedOutAsync(payload, cancellationToken);
                break;
            default:
                logger.LogWarning("Ignoring unhandled event type {EventType}", eventType);
                break;
        }
    }
}

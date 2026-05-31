using Cart.Infrastructure.Projections;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Options;
using CqrsDemo.Contracts.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cart.Projection.Worker;

public sealed class CartProjectionProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<AzureServiceBusOptions> options,
    ILogger<CartProjectionProcessor> logger)
    : ServiceBusConsumerBackgroundService(options, logger)
{
    protected override string SubscriptionName => ServiceBusSubscriptions.CartProjection;

    protected override async Task ProcessMessageAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CartProjectionHandler>().HandleAsync(eventType, payload, cancellationToken);
    }
}

using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Options;
using CqrsDemo.Contracts.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Product.Infrastructure.Projections;

namespace Product.Projection.Worker;

public sealed class ProductProjectionProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<AzureServiceBusOptions> options,
    ILogger<ProductProjectionProcessor> logger) : ServiceBusConsumerBackgroundService(options, logger)
{
    protected override string SubscriptionName => ServiceBusSubscriptions.ProductProjection;

    protected override async Task ProcessMessageAsync(string eventType, string payload, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ProductProjectionHandler>().HandleAsync(eventType, payload, ct);
    }
}

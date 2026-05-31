using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Options;
using CqrsDemo.Contracts.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payment.Infrastructure.Projections;

namespace Payment.Projection.Worker;

public sealed class PaymentProjectionProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<AzureServiceBusOptions> options,
    ILogger<PaymentProjectionProcessor> logger)
    : ServiceBusConsumerBackgroundService(options, logger)
{
    protected override string SubscriptionName => ServiceBusSubscriptions.PaymentProjection;

    protected override async Task ProcessMessageAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PaymentProjectionHandler>().HandleAsync(eventType, payload, cancellationToken);
    }
}

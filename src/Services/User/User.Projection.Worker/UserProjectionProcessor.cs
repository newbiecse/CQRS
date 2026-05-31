using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Options;
using CqrsDemo.Contracts.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using User.Infrastructure.Projections;

namespace User.Projection.Worker;

public sealed class UserProjectionProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<AzureServiceBusOptions> options,
    ILogger<UserProjectionProcessor> logger)
    : ServiceBusConsumerBackgroundService(options, logger)
{
    protected override string SubscriptionName => ServiceBusSubscriptions.UserProjection;

    protected override async Task ProcessMessageAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        await scope.ServiceProvider.GetRequiredService<UserProjectionHandler>()
            .HandleAsync(eventType, payload, cancellationToken);
    }
}

using Azure.Messaging.ServiceBus.Administration;
using CqrsDemo.BuildingBlocks.Messaging.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CqrsDemo.BuildingBlocks.Messaging;

public sealed class ServiceBusTopologyInitializer(IOptions<AzureServiceBusOptions> options, ILogger<ServiceBusTopologyInitializer> logger)
{
    public async Task EnsureTopicAndSubscriptionsAsync(IEnumerable<string> subscriptionNames, CancellationToken ct = default)
    {
        var s = options.Value;
        var admin = new ServiceBusAdministrationClient(s.ConnectionString);
        if (!await admin.TopicExistsAsync(s.TopicName, ct))
        {
            await admin.CreateTopicAsync(s.TopicName, ct);
            logger.LogInformation("Created topic {Topic}", s.TopicName);
        }
        foreach (var sub in subscriptionNames)
        {
            if (!await admin.SubscriptionExistsAsync(s.TopicName, sub, ct))
            {
                await admin.CreateSubscriptionAsync(s.TopicName, sub, ct);
                logger.LogInformation("Created subscription {Sub}", sub);
            }
        }
    }
}

using Azure.Messaging.ServiceBus.Administration;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Messaging.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CqrsDemo.Messaging;

public sealed class ServiceBusTopologyInitializer(
    IOptions<AzureServiceBusOptions> options,
    ILogger<ServiceBusTopologyInitializer> logger)
{
    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var adminClient = new ServiceBusAdministrationClient(settings.ConnectionString);

        if (!await adminClient.TopicExistsAsync(settings.TopicName, cancellationToken))
        {
            await adminClient.CreateTopicAsync(settings.TopicName, cancellationToken: cancellationToken);
            logger.LogInformation("Created Service Bus topic {TopicName}", settings.TopicName);
        }

        if (!await adminClient.SubscriptionExistsAsync(
                settings.TopicName,
                ServiceBusSubscriptions.ProductProjection,
                cancellationToken))
        {
            await adminClient.CreateSubscriptionAsync(
                settings.TopicName,
                ServiceBusSubscriptions.ProductProjection,
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Created Service Bus subscription {Subscription} on topic {TopicName}",
                ServiceBusSubscriptions.ProductProjection,
                settings.TopicName);
        }
    }
}

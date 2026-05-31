using Azure.Messaging.ServiceBus;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Messaging.Abstractions;
using CqrsDemo.Messaging.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CqrsDemo.Messaging;

public sealed class AzureServiceBusIntegrationEventPublisher : IIntegrationEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<AzureServiceBusIntegrationEventPublisher> _logger;

    public AzureServiceBusIntegrationEventPublisher(
        IOptions<AzureServiceBusOptions> options,
        ILogger<AzureServiceBusIntegrationEventPublisher> logger)
    {
        _logger = logger;
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new InvalidOperationException(
                "Azure Service Bus connection string is not configured. Set AzureServiceBus:ConnectionString.");
        }

        _client = new ServiceBusClient(settings.ConnectionString);
        _sender = _client.CreateSender(settings.TopicName);
    }

    public async Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        var message = new ServiceBusMessage(envelope.Payload)
        {
            ContentType = "application/json",
            Subject = envelope.EventType,
            MessageId = Guid.NewGuid().ToString()
        };

        message.ApplicationProperties["EventType"] = envelope.EventType;

        await _sender.SendMessageAsync(message, cancellationToken);

        _logger.LogInformation(
            "Published integration event {EventType} to Service Bus topic",
            envelope.EventType);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}

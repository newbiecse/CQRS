using Azure.Messaging.ServiceBus;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.BuildingBlocks.Messaging.Options;
using CqrsDemo.Contracts.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CqrsDemo.BuildingBlocks.Messaging;

public sealed class AzureServiceBusIntegrationEventPublisher : IIntegrationEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<AzureServiceBusIntegrationEventPublisher> _logger;

    public AzureServiceBusIntegrationEventPublisher(IOptions<AzureServiceBusOptions> options, ILogger<AzureServiceBusIntegrationEventPublisher> logger)
    {
        _logger = logger;
        var s = options.Value;
        if (string.IsNullOrWhiteSpace(s.ConnectionString))
            throw new InvalidOperationException("AzureServiceBus:ConnectionString is required.");
        _client = new ServiceBusClient(s.ConnectionString);
        _sender = _client.CreateSender(s.TopicName);
    }

    public async Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken ct = default)
    {
        var message = new ServiceBusMessage(envelope.Payload)
        {
            ContentType = "application/json",
            Subject = envelope.EventType,
            MessageId = Guid.NewGuid().ToString()
        };
        message.ApplicationProperties["EventType"] = envelope.EventType;
        await _sender.SendMessageAsync(message, ct);
        _logger.LogInformation("Published {EventType}", envelope.EventType);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}

using Azure.Messaging.ServiceBus;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Messaging.Options;
using CqrsDemo.Projection.Worker.Projections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CqrsDemo.Projection.Worker.ServiceBus;

public sealed class ProductProjectionProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<AzureServiceBusOptions> options,
    ILogger<ProductProjectionProcessor> logger) : BackgroundService
{
    private ServiceBusProcessor? _processor;
    private ServiceBusClient? _client;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new InvalidOperationException(
                "Azure Service Bus connection string is not configured. Set AzureServiceBus:ConnectionString.");
        }

        _client = new ServiceBusClient(settings.ConnectionString);
        _processor = _client.CreateProcessor(
            settings.TopicName,
            ServiceBusSubscriptions.ProductProjection,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });

        _processor.ProcessMessageAsync += OnMessageAsync;
        _processor.ProcessErrorAsync += OnErrorAsync;

        logger.LogInformation(
            "Listening on topic {Topic}, subscription {Subscription}",
            settings.TopicName,
            ServiceBusSubscriptions.ProductProjection);

        await _processor.StartProcessingAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }

        if (_client is not null)
        {
            await _client.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task OnMessageAsync(ProcessMessageEventArgs args)
    {
        var eventType = args.Message.Subject;
        if (string.IsNullOrWhiteSpace(eventType)
            && args.Message.ApplicationProperties.TryGetValue("EventType", out var eventTypeProperty))
        {
            eventType = eventTypeProperty?.ToString();
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            logger.LogWarning("Received message without EventType. Dead-lettering message {MessageId}.", args.Message.MessageId);
            await args.DeadLetterMessageAsync(args.Message, "MissingEventType", "Message Subject/EventType is required.");
            return;
        }

        var payload = args.Message.Body.ToString();

        try
        {
            using var scope = scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ShopProjectionHandler>();
            await handler.HandleAsync(eventType, payload, args.CancellationToken);
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to process message {MessageId} with event type {EventType}",
                args.Message.MessageId,
                eventType);

            await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
        }
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(
            args.Exception,
            "Service Bus processor error. Source={ErrorSource}, Entity={EntityPath}",
            args.ErrorSource,
            args.EntityPath);

        return Task.CompletedTask;
    }
}

using Azure.Messaging.ServiceBus;
using CqrsDemo.BuildingBlocks.Messaging.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CqrsDemo.BuildingBlocks.Messaging;

public abstract class ServiceBusConsumerBackgroundService(
    IOptions<AzureServiceBusOptions> options,
    ILogger logger) : BackgroundService
{
    protected abstract string SubscriptionName { get; }
    protected abstract Task ProcessMessageAsync(string eventType, string payload, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var s = options.Value;
        await using var client = new ServiceBusClient(s.ConnectionString);
        var processor = client.CreateProcessor(s.TopicName, SubscriptionName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });

        processor.ProcessMessageAsync += async args =>
        {
            var eventType = args.Message.Subject
                ?? args.Message.ApplicationProperties.GetValueOrDefault("EventType")?.ToString();
            if (string.IsNullOrWhiteSpace(eventType))
            {
                await args.DeadLetterMessageAsync(args.Message, "MissingEventType", "EventType required", cancellationToken: args.CancellationToken);
                return;
            }
            try
            {
                await ProcessMessageAsync(eventType, args.Message.Body.ToString(), args.CancellationToken);
                await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed processing {EventType}", eventType);
                await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            logger.LogError(args.Exception, "Service Bus error on {Sub}", SubscriptionName);
            return Task.CompletedTask;
        };

        logger.LogInformation("Listening {Topic}/{Sub}", s.TopicName, SubscriptionName);
        await processor.StartProcessingAsync(stoppingToken);
        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        await processor.StopProcessingAsync(stoppingToken);
    }
}

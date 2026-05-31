using CqrsDemo.Commands.Infrastructure.Persistence.Write;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CqrsDemo.Commands.Infrastructure.Outbox;

public sealed class OutboxPublisherBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxPublisherBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private const int BatchSize = 20;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox publisher started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var publishedCount = await PublishPendingMessagesAsync(stoppingToken);

                if (publishedCount == 0)
                {
                    await Task.Delay(PollInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox publisher encountered an error.");
                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        logger.LogInformation("Outbox publisher stopped.");
    }

    private async Task<int> PublishPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();

        var pendingMessages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.OccurredOn)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (pendingMessages.Count == 0)
        {
            return 0;
        }

        foreach (var message in pendingMessages)
        {
            try
            {
                await publisher.PublishAsync(
                    new IntegrationEventEnvelope(message.EventType, message.Payload),
                    cancellationToken);

                message.ProcessedAt = DateTime.UtcNow;
                message.LastError = null;
            }
            catch (Exception ex)
            {
                message.AttemptCount++;
                message.LastError = ex.Message;
                logger.LogWarning(
                    ex,
                    "Failed to publish outbox message {OutboxId} (attempt {Attempt})",
                    message.Id,
                    message.AttemptCount);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return pendingMessages.Count;
    }
}

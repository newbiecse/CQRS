using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.BuildingBlocks.Messaging.Persistence;
using CqrsDemo.Contracts.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CqrsDemo.BuildingBlocks.Messaging;

public sealed class OutboxPublisherBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxPublisherBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var count = await PublishBatchAsync(stoppingToken);
                if (count == 0) await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox publish error");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    private async Task<int> PublishBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IOutboxDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();
        var batch = await db.OutboxMessages.Where(m => m.ProcessedAt == null).OrderBy(m => m.OccurredOn).Take(20).ToListAsync(ct);
        if (batch.Count == 0) return 0;
        foreach (var m in batch)
        {
            try
            {
                await publisher.PublishAsync(new IntegrationEventEnvelope(m.EventType, m.Payload), ct);
                m.ProcessedAt = DateTime.UtcNow;
                m.LastError = null;
            }
            catch (Exception ex)
            {
                m.AttemptCount++;
                m.LastError = ex.Message;
            }
        }
        await db.SaveChangesAsync(ct);
        return batch.Count;
    }
}

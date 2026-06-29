using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.BuildingBlocks.Messaging.Persistence;
using CqrsDemo.Contracts.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CqrsDemo.BuildingBlocks.Messaging;

public static class OutboxPersistence
{
    public static void AddOutboxMessages(
        this IOutboxDbContext db,
        IIntegrationEventMapper mapper,
        IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var message in mapper.Map(domainEvents))
        {
            db.OutboxMessages.Add(new OutboxMessageEntity
            {
                Id = Guid.NewGuid(),
                EventType = message.EventType,
                Payload = message.Payload,
                CorrelationId = CorrelationContext.CorrelationId ?? CorrelationContext.GetOrCreateCorrelationId(),
                TraceId = CorrelationContext.TraceId ?? Activity.Current?.TraceId.ToString(),
                OccurredOn = DateTime.UtcNow,
                AttemptCount = 0
            });
        }
    }

    public static void ConfigureOutbox(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessageEntity>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.EventType).HasMaxLength(200).IsRequired();
            entity.Property(o => o.Payload).IsRequired();
            entity.Property(o => o.CorrelationId).HasMaxLength(64);
            entity.Property(o => o.TraceId).HasMaxLength(64);
            entity.Property(o => o.LastError).HasMaxLength(2000);
            entity.HasIndex(o => new { o.ProcessedAt, o.OccurredOn });
        });
    }
}

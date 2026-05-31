using CqrsDemo.Commands.Infrastructure.Persistence.EventStore;
using CqrsDemo.Commands.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace CqrsDemo.Commands.Infrastructure.Persistence.Write;

public sealed class WriteDbContext(DbContextOptions<WriteDbContext> options) : DbContext(options)
{
    public DbSet<StoredEventEntity> StoredEvents => Set<StoredEventEntity>();
    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoredEventEntity>(entity =>
        {
            entity.ToTable("StoredEvents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StreamType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EventType).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.OccurredOn).IsRequired();
            entity.HasIndex(e => new { e.StreamId, e.StreamType, e.Version }).IsUnique();
            entity.HasIndex(e => new { e.StreamId, e.StreamType });
        });

        modelBuilder.Entity<OutboxMessageEntity>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.EventType).HasMaxLength(200).IsRequired();
            entity.Property(o => o.Payload).IsRequired();
            entity.Property(o => o.OccurredOn).IsRequired();
            entity.Property(o => o.LastError).HasMaxLength(2000);
            entity.HasIndex(o => new { o.ProcessedAt, o.OccurredOn });
        });
    }
}

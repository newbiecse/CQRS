using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.BuildingBlocks.Messaging.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Payment.Infrastructure.Persistence.Write;

public sealed class PaymentWriteDbContext(DbContextOptions<PaymentWriteDbContext> options)
    : DbContext(options), IOutboxDbContext
{
    public DbSet<PaymentWriteEntity> Payments => Set<PaymentWriteEntity>();
    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentWriteEntity>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Amount).HasPrecision(18, 2);
            entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(p => p.FailureReason).HasMaxLength(500);
        });

        OutboxPersistence.ConfigureOutbox(modelBuilder);
    }
}

public sealed class PaymentWriteEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime InitiatedAt { get; set; }
    public string? FailureReason { get; set; }
}

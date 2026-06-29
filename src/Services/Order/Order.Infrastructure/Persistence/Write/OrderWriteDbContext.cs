using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.BuildingBlocks.Messaging.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Order.Infrastructure.Persistence.Write;

public sealed class OrderWriteDbContext(DbContextOptions<OrderWriteDbContext> options)
    : DbContext(options), IOutboxDbContext
{
    public DbSet<OrderWriteEntity> Orders => Set<OrderWriteEntity>();
    public DbSet<OrderLineWriteEntity> OrderLines => Set<OrderLineWriteEntity>();
    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderWriteEntity>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderLineWriteEntity>(entity =>
        {
            entity.ToTable("OrderLines");
            entity.HasKey(l => new { l.OrderId, l.ProductId });
            entity.Property(l => l.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(l => l.UnitPrice).HasPrecision(18, 2);
            entity.HasOne<OrderWriteEntity>()
                .WithMany()
                .HasForeignKey(l => l.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        OutboxPersistence.ConfigureOutbox(modelBuilder);
    }
}

public sealed class OrderWriteEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid CartId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class OrderLineWriteEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

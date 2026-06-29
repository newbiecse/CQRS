using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.BuildingBlocks.Messaging.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CqrsDemo.Commands.Infrastructure.Persistence.Write;

public sealed class WriteDbContext(DbContextOptions<WriteDbContext> options)
    : DbContext(options), IOutboxDbContext
{
    public DbSet<ProductWriteEntity> Products => Set<ProductWriteEntity>();
    public DbSet<CartWriteEntity> Carts => Set<CartWriteEntity>();
    public DbSet<CartItemWriteEntity> CartItems => Set<CartItemWriteEntity>();
    public DbSet<OrderWriteEntity> Orders => Set<OrderWriteEntity>();
    public DbSet<OrderLineWriteEntity> OrderLines => Set<OrderLineWriteEntity>();
    public DbSet<PaymentWriteEntity> Payments => Set<PaymentWriteEntity>();
    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductWriteEntity>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<CartWriteEntity>(entity =>
        {
            entity.ToTable("Carts");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Status).HasConversion<string>().HasMaxLength(50);
        });

        modelBuilder.Entity<CartItemWriteEntity>(entity =>
        {
            entity.ToTable("CartItems");
            entity.HasKey(i => new { i.CartId, i.ProductId });
            entity.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            entity.HasOne<CartWriteEntity>()
                .WithMany()
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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

public sealed class ProductWriteEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class CartWriteEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class CartItemWriteEntity
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
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

public sealed class PaymentWriteEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime InitiatedAt { get; set; }
    public string? FailureReason { get; set; }
}

using Inventory.Application.Abstractions;
using Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<InventoryItemEntity> InventoryItems => Set<InventoryItemEntity>();
    public DbSet<OrderReservationEntity> OrderReservations => Set<OrderReservationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItemEntity>(entity =>
        {
            entity.ToTable("InventoryItems");
            entity.HasKey(i => i.ProductId);
            entity.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<OrderReservationEntity>(entity =>
        {
            entity.ToTable("OrderReservations");
            entity.HasKey(r => new { r.OrderId, r.ProductId });
            entity.HasIndex(r => r.OrderId);
        });
    }
}

public sealed class InventoryItemEntity
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int OnHand { get; set; }
    public int Reserved { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}

public sealed class OrderReservationEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.BuildingBlocks.Messaging.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cart.Infrastructure.Persistence.Write;

public sealed class CartWriteDbContext(DbContextOptions<CartWriteDbContext> options)
    : DbContext(options), IOutboxDbContext
{
    public DbSet<CartWriteEntity> Carts => Set<CartWriteEntity>();
    public DbSet<CartItemWriteEntity> CartItems => Set<CartItemWriteEntity>();
    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        OutboxPersistence.ConfigureOutbox(modelBuilder);
    }
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

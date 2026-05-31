using Microsoft.EntityFrameworkCore;

namespace CqrsDemo.Queries.Infrastructure.Persistence.Read;

public sealed class ReadDbContext(DbContextOptions<ReadDbContext> options) : DbContext(options)
{
    public DbSet<ProductReadEntity> Products => Set<ProductReadEntity>();
    public DbSet<CartReadEntity> Carts => Set<CartReadEntity>();
    public DbSet<CartLineReadEntity> CartLines => Set<CartLineReadEntity>();
    public DbSet<OrderReadEntity> Orders => Set<OrderReadEntity>();
    public DbSet<OrderLineReadEntity> OrderLines => Set<OrderLineReadEntity>();
    public DbSet<PaymentReadEntity> Payments => Set<PaymentReadEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductReadEntity>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.CreatedAt).IsRequired();
            entity.Property(p => p.LastUpdatedAt).IsRequired();
            entity.HasIndex(p => p.Name);
        });

        modelBuilder.Entity<CartReadEntity>(entity =>
        {
            entity.ToTable("Carts");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Status).HasMaxLength(50).IsRequired();
            entity.Property(c => c.Subtotal).HasPrecision(18, 2);
            entity.HasMany(c => c.Lines)
                .WithOne(l => l.Cart)
                .HasForeignKey(l => l.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartLineReadEntity>(entity =>
        {
            entity.ToTable("CartLines");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(l => l.UnitPrice).HasPrecision(18, 2);
            entity.HasIndex(l => new { l.CartId, l.ProductId }).IsUnique();
        });

        modelBuilder.Entity<OrderReadEntity>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Status).HasMaxLength(50).IsRequired();
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            entity.HasMany(o => o.Lines)
                .WithOne(l => l.Order)
                .HasForeignKey(l => l.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderLineReadEntity>(entity =>
        {
            entity.ToTable("OrderLines");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(l => l.UnitPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PaymentReadEntity>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Status).HasMaxLength(50).IsRequired();
            entity.Property(p => p.Amount).HasPrecision(18, 2);
            entity.Property(p => p.FailureReason).HasMaxLength(500);
            entity.HasIndex(p => p.OrderId);
        });
    }
}

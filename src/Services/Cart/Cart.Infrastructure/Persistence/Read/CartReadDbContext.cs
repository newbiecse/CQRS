using Microsoft.EntityFrameworkCore;

namespace Cart.Infrastructure.Persistence.Read;

public sealed class CartReadDbContext(DbContextOptions<CartReadDbContext> options) : DbContext(options)
{
    public DbSet<CartReadEntity> Carts => Set<CartReadEntity>();
    public DbSet<CartLineReadEntity> CartLines => Set<CartLineReadEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
    }
}

public sealed class CartReadEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public ICollection<CartLineReadEntity> Lines { get; set; } = [];
}

public sealed class CartLineReadEntity
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public CartReadEntity? Cart { get; set; }
}

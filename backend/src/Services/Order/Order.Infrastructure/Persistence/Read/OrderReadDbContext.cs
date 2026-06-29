using Microsoft.EntityFrameworkCore;

namespace Order.Infrastructure.Persistence.Read;

public sealed class OrderReadDbContext(DbContextOptions<OrderReadDbContext> options) : DbContext(options)
{
    public DbSet<OrderReadEntity> Orders => Set<OrderReadEntity>();
    public DbSet<OrderLineReadEntity> OrderLines => Set<OrderLineReadEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
    }
}

public sealed class OrderReadEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid CartId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public Guid? PaymentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public ICollection<OrderLineReadEntity> Lines { get; set; } = [];
}

public sealed class OrderLineReadEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public OrderReadEntity? Order { get; set; }
}

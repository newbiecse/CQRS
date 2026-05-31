using Microsoft.EntityFrameworkCore;

namespace Product.Infrastructure.Persistence.Read;

public sealed class ProductReadDbContext(DbContextOptions<ProductReadDbContext> options) : DbContext(options)
{
    public DbSet<ProductReadEntity> Products => Set<ProductReadEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductReadEntity>(e =>
        {
            e.ToTable("Products");
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.Price).HasPrecision(18, 2);
        });
    }
}

public sealed class ProductReadEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}

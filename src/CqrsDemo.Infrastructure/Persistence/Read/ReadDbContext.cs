using Microsoft.EntityFrameworkCore;

namespace CqrsDemo.Infrastructure.Persistence.Read;

public sealed class ReadDbContext(DbContextOptions<ReadDbContext> options) : DbContext(options)
{
    public DbSet<ProductReadEntity> Products => Set<ProductReadEntity>();

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
    }
}

using Microsoft.EntityFrameworkCore;

namespace CqrsDemo.Infrastructure.Persistence.Write;

public sealed class WriteDbContext(DbContextOptions<WriteDbContext> options) : DbContext(options)
{
    public DbSet<ProductWriteEntity> Products => Set<ProductWriteEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductWriteEntity>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.CreatedAt).IsRequired();
        });
    }
}

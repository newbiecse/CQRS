using Microsoft.EntityFrameworkCore;

namespace Payment.Infrastructure.Persistence.Read;

public sealed class PaymentReadDbContext(DbContextOptions<PaymentReadDbContext> options) : DbContext(options)
{
    public DbSet<PaymentReadEntity> Payments => Set<PaymentReadEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

public sealed class PaymentReadEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public DateTime InitiatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}

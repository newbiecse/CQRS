using Microsoft.EntityFrameworkCore;

namespace Reporting.Infrastructure.Persistence;

public sealed class ReportingDbContext(DbContextOptions<ReportingDbContext> options) : DbContext(options)
{
    public DbSet<UserProfileReportEntity> UserProfiles => Set<UserProfileReportEntity>();
    public DbSet<OrderReportFactEntity> OrderFacts => Set<OrderReportFactEntity>();
    public DbSet<OrderLineFactEntity> OrderLineFacts => Set<OrderLineFactEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfileReportEntity>(entity =>
        {
            entity.ToTable("UserProfiles");
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.Email).HasMaxLength(320).IsRequired();
            entity.Property(u => u.DisplayName).HasMaxLength(200).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<OrderReportFactEntity>(entity =>
        {
            entity.ToTable("OrderFacts");
            entity.HasKey(o => o.OrderId);
            entity.Property(o => o.UserEmail).HasMaxLength(320);
            entity.Property(o => o.UserDisplayName).HasMaxLength(200);
            entity.Property(o => o.Status).HasMaxLength(50).IsRequired();
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            entity.HasIndex(o => new { o.UserId, o.OrderCreatedAt });
            entity.HasIndex(o => o.OrderCreatedAt);
        });

        modelBuilder.Entity<OrderLineFactEntity>(entity =>
        {
            entity.ToTable("OrderLineFacts");
            entity.HasKey(l => new { l.OrderId, l.ProductId });
            entity.Property(l => l.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(l => l.UnitPrice).HasPrecision(18, 2);
            entity.Property(l => l.LineTotal).HasPrecision(18, 2);
            entity.HasIndex(l => new { l.ProductId, l.OrderCreatedAt });
            entity.HasIndex(l => l.OrderCreatedAt);
        });
    }
}

public sealed class UserProfileReportEntity
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}

public sealed class OrderReportFactEntity
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserDisplayName { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderCreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}

public sealed class OrderLineFactEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public DateTime OrderCreatedAt { get; set; }
}

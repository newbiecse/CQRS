using Microsoft.EntityFrameworkCore;

namespace User.Infrastructure.Persistence.Read;

public sealed class UserReadDbContext(DbContextOptions<UserReadDbContext> options) : DbContext(options)
{
    public DbSet<UserReadEntity> Users => Set<UserReadEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserReadEntity>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).HasMaxLength(320).IsRequired();
            entity.Property(u => u.DisplayName).HasMaxLength(200).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
        });
    }
}

public sealed class UserReadEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}

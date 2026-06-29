using Auth.Domain;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<IdentityUserEntity> Users => Set<IdentityUserEntity>();
    public DbSet<LocalCredentialEntity> LocalCredentials => Set<LocalCredentialEntity>();
    public DbSet<ExternalLoginEntity> ExternalLogins => Set<ExternalLoginEntity>();
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();
    public DbSet<RolePermissionEntity> RolePermissions => Set<RolePermissionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityUserEntity>(entity =>
        {
            entity.ToTable("IdentityUsers");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).HasMaxLength(320).IsRequired();
            entity.Property(u => u.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(u => u.RolesCsv).HasMaxLength(500).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<LocalCredentialEntity>(entity =>
        {
            entity.ToTable("LocalCredentials");
            entity.HasKey(c => c.UserId);
            entity.Property(c => c.PasswordHash).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<ExternalLoginEntity>(entity =>
        {
            entity.ToTable("ExternalLogins");
            entity.HasKey(e => new { e.Provider, e.ProviderUserId });
            entity.Property(e => e.Provider).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ProviderUserId).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<RoleEntity>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).HasMaxLength(64).IsRequired();
            entity.Property(r => r.Description).HasMaxLength(256).IsRequired();
            entity.HasIndex(r => r.Name).IsUnique();
        });

        modelBuilder.Entity<PermissionEntity>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(128).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(256).IsRequired();
            entity.HasIndex(p => p.Name).IsUnique();
        });

        modelBuilder.Entity<RolePermissionEntity>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            entity.HasOne<RoleEntity>().WithMany().HasForeignKey(rp => rp.RoleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<PermissionEntity>().WithMany().HasForeignKey(rp => rp.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}

public sealed class IdentityUserEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string RolesCsv { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class LocalCredentialEntity
{
    public Guid UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
}

public sealed class ExternalLoginEntity
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime LinkedAt { get; set; }
}

public sealed class RoleEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class PermissionEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class RolePermissionEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
}

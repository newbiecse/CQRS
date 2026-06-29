using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.BuildingBlocks.Messaging.Persistence;
using Microsoft.EntityFrameworkCore;

namespace User.Infrastructure.Persistence.Write;

public sealed class UserWriteDbContext(DbContextOptions<UserWriteDbContext> options)
    : DbContext(options), IOutboxDbContext
{
    public DbSet<UserWriteEntity> Users => Set<UserWriteEntity>();
    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserWriteEntity>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).HasMaxLength(320).IsRequired();
            entity.Property(u => u.DisplayName).HasMaxLength(200).IsRequired();
        });

        OutboxPersistence.ConfigureOutbox(modelBuilder);
    }
}

public sealed class UserWriteEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }
}

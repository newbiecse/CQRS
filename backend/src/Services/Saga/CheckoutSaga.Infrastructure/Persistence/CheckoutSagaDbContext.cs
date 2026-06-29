using CheckoutSaga.Domain;
using Microsoft.EntityFrameworkCore;

namespace CheckoutSaga.Infrastructure.Persistence;

public sealed class CheckoutSagaDbContext(DbContextOptions<CheckoutSagaDbContext> options) : DbContext(options)
{
    public DbSet<CheckoutSagaInstance> Sagas => Set<CheckoutSagaInstance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CheckoutSagaInstance>(entity =>
        {
            entity.ToTable("CheckoutSagas");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.State).HasMaxLength(64).IsRequired();
            entity.Property(s => s.FailureReason).HasMaxLength(2000);
            entity.HasIndex(s => s.CartId);
            entity.HasIndex(s => s.OrderId);
        });
    }
}

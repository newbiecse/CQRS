using Microsoft.EntityFrameworkCore;
using Payment.Application.Abstractions;
using Payment.Application.ReadModels;

namespace Payment.Infrastructure.Persistence.Read;

public sealed class SqlPaymentReadRepository(PaymentReadDbContext dbContext) : IPaymentReadRepository
{
    public async Task UpsertAsync(PaymentReadModel payment, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Payments.FirstOrDefaultAsync(p => p.Id == payment.Id, cancellationToken);
        if (entity is null)
        {
            entity = new PaymentReadEntity { Id = payment.Id };
            dbContext.Payments.Add(entity);
        }

        entity.OrderId = payment.OrderId;
        entity.Amount = payment.Amount;
        entity.Status = payment.Status;
        entity.FailureReason = payment.FailureReason;
        entity.InitiatedAt = payment.InitiatedAt;
        entity.LastUpdatedAt = payment.LastUpdatedAt;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaymentReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return entity is null ? null : ToReadModel(entity);
    }

    public async Task<PaymentReadModel?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
        return entity is null ? null : ToReadModel(entity);
    }

    private static PaymentReadModel ToReadModel(PaymentReadEntity entity) => new()
    {
        Id = entity.Id,
        OrderId = entity.OrderId,
        Amount = entity.Amount,
        Status = entity.Status,
        FailureReason = entity.FailureReason,
        InitiatedAt = entity.InitiatedAt,
        LastUpdatedAt = entity.LastUpdatedAt
    };
}

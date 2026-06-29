using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Abstractions;
using Payment.Domain;

namespace Payment.Infrastructure.Persistence.Write;

public sealed class SqlPaymentWriteRepository(
    PaymentWriteDbContext db,
    IIntegrationEventMapper integrationEventMapper) : IPaymentWriteRepository
{
    public async Task AddAsync(PaymentAggregate payment, CancellationToken cancellationToken = default)
    {
        db.Payments.Add(ToEntity(payment));
        db.AddOutboxMessages(integrationEventMapper, payment.DomainEvents);
        payment.ClearDomainEvents();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaymentAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return entity is null ? null : ToAggregate(entity);
    }

    public async Task UpdateAsync(PaymentAggregate payment, CancellationToken cancellationToken = default)
    {
        var entity = await db.Payments.FindAsync([payment.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"Payment {payment.Id} was not found.");

        entity.OrderId = payment.OrderId;
        entity.Amount = payment.Amount;
        entity.Status = payment.Status.ToString();
        entity.InitiatedAt = payment.InitiatedAt;
        entity.FailureReason = payment.FailureReason;

        db.AddOutboxMessages(integrationEventMapper, payment.DomainEvents);
        payment.ClearDomainEvents();
        await db.SaveChangesAsync(cancellationToken);
    }

    private static PaymentWriteEntity ToEntity(PaymentAggregate payment) =>
        new()
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            InitiatedAt = payment.InitiatedAt,
            FailureReason = payment.FailureReason
        };

    private static PaymentAggregate ToAggregate(PaymentWriteEntity entity) =>
        PaymentAggregate.Restore(
            entity.Id,
            entity.OrderId,
            entity.Amount,
            Enum.Parse<PaymentStatus>(entity.Status),
            entity.InitiatedAt,
            entity.FailureReason);
}

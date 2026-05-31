using CheckoutSaga.Application.Abstractions;
using CheckoutSaga.Domain;
using Microsoft.EntityFrameworkCore;

namespace CheckoutSaga.Infrastructure.Persistence;

public sealed class SqlCheckoutSagaRepository(CheckoutSagaDbContext db) : ICheckoutSagaRepository
{
    public async Task AddAsync(CheckoutSagaInstance saga, CancellationToken cancellationToken)
    {
        db.Sagas.Add(saga);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<CheckoutSagaInstance?> GetByIdAsync(Guid sagaId, CancellationToken cancellationToken) =>
        db.Sagas.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sagaId, cancellationToken);

    public Task<CheckoutSagaInstance?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken) =>
        db.Sagas.FirstOrDefaultAsync(s => s.OrderId == orderId, cancellationToken);

    public Task<CheckoutSagaInstance?> GetByCartIdAsync(Guid cartId, CancellationToken cancellationToken) =>
        db.Sagas.OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(s => s.CartId == cartId, cancellationToken);

    public async Task UpdateAsync(CheckoutSagaInstance saga, CancellationToken cancellationToken)
    {
        db.Sagas.Update(saga);
        await db.SaveChangesAsync(cancellationToken);
    }
}

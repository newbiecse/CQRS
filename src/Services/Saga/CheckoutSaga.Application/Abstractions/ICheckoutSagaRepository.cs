using CheckoutSaga.Domain;

namespace CheckoutSaga.Application.Abstractions;

public interface ICheckoutSagaRepository
{
    Task AddAsync(CheckoutSagaInstance saga, CancellationToken cancellationToken);
    Task<CheckoutSagaInstance?> GetByIdAsync(Guid sagaId, CancellationToken cancellationToken);
    Task<CheckoutSagaInstance?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);
    Task<CheckoutSagaInstance?> GetByCartIdAsync(Guid cartId, CancellationToken cancellationToken);
    Task UpdateAsync(CheckoutSagaInstance saga, CancellationToken cancellationToken);
}

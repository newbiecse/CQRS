using Cart.Domain;

namespace Cart.Application.Abstractions;

public interface ICartWriteRepository
{
    Task AddAsync(CartAggregate cart, CancellationToken cancellationToken = default);
    Task<CartAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(CartAggregate cart, CancellationToken cancellationToken = default);
}

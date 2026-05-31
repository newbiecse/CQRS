using Cart.Application.ReadModels;

namespace Cart.Application.Abstractions;

public interface ICartReadRepository
{
    Task UpsertAsync(CartReadModel cart, CancellationToken cancellationToken = default);
    Task<CartReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

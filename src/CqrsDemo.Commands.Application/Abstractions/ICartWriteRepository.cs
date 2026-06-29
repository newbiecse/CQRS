using CqrsDemo.Domain.Carts;

namespace CqrsDemo.Commands.Application.Abstractions;

public interface ICartWriteRepository
{
    Task AddAsync(Cart cart, CancellationToken cancellationToken = default);
    Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default);
}

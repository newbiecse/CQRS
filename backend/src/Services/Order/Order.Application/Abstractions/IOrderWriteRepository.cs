using Order.Domain;

namespace Order.Application.Abstractions;

public interface IOrderWriteRepository
{
    Task AddAsync(OrderAggregate order, CancellationToken cancellationToken = default);
    Task<OrderAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(OrderAggregate order, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid orderId, CancellationToken cancellationToken = default);
}

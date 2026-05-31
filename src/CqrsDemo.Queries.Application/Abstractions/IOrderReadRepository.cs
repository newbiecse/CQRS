using CqrsDemo.Queries.Application.Orders.ReadModels;

namespace CqrsDemo.Queries.Application.Abstractions;

public interface IOrderReadRepository
{
    Task UpsertAsync(OrderReadModel order, CancellationToken cancellationToken = default);
    Task<OrderReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderReadModel>> GetAllAsync(CancellationToken cancellationToken = default);
}

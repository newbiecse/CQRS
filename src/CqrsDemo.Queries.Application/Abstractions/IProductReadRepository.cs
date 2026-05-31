using CqrsDemo.Queries.Application.Products.ReadModels;

namespace CqrsDemo.Queries.Application.Abstractions;

public interface IProductReadRepository
{
    Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductReadModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpsertAsync(ProductReadModel product, CancellationToken cancellationToken = default);
}

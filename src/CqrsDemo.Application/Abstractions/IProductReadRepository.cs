using CqrsDemo.Application.Products.ReadModels;

namespace CqrsDemo.Application.Abstractions;

public interface IProductReadRepository
{
    Task UpsertAsync(ProductReadModel product, CancellationToken cancellationToken = default);
    Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductReadModel>> GetAllAsync(CancellationToken cancellationToken = default);
}

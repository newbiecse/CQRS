using Product.Application.ReadModels;

namespace Product.Application.Abstractions;

public interface IProductReadRepository
{
    Task UpsertAsync(ProductReadModel product, CancellationToken ct = default);
    Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ProductReadModel>> GetAllAsync(CancellationToken ct = default);
}

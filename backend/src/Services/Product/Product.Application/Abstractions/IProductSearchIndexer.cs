using Product.Application.ReadModels;

namespace Product.Application.Abstractions;

public interface IProductSearchIndexer
{
    Task IndexAsync(ProductReadModel product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid productId, CancellationToken cancellationToken = default);
}

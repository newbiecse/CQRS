using Product.Application.ReadModels;

namespace Product.Application.Abstractions;

public interface IProductSearchIndexer
{
    Task IndexAsync(ProductReadModel product, CancellationToken cancellationToken = default);
}

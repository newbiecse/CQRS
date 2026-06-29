using Product.Application.ReadModels;

namespace Product.Application.Abstractions;

public interface IProductSearchReader
{
    Task<IReadOnlyList<ProductSearchResult>> SearchAsync(
        string query,
        int size,
        CancellationToken cancellationToken = default);
}

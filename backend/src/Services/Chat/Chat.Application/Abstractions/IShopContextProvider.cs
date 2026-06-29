using Chat.Application.Models;

namespace Chat.Application.Abstractions;

public interface IShopContextProvider
{
    Task<IReadOnlyList<ProductContextItem>> GetCatalogAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductContextItem>> SearchProductsAsync(string query, CancellationToken cancellationToken = default);
}

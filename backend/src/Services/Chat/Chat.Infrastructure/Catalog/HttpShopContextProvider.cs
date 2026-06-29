using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Chat.Application.Abstractions;
using Chat.Application.Models;
using Microsoft.Extensions.Options;
using Chat.Application.Options;

namespace Chat.Infrastructure.Catalog;

internal sealed record ProductApiDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("price")] decimal Price);

internal sealed record ProductSearchApiDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("price")] decimal Price);

public sealed class HttpShopContextProvider(
    HttpClient httpClient,
    IOptions<ChatAgentOptions> options) : IShopContextProvider
{
    public async Task<IReadOnlyList<ProductContextItem>> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        var baseUrl = options.Value.ProductQueriesBaseUrl.TrimEnd('/');
        var products = await httpClient.GetFromJsonAsync<List<ProductApiDto>>(
            $"{baseUrl}/api/products",
            cancellationToken);

        return (products ?? [])
            .Select(p => new ProductContextItem(p.Id, p.Name, p.Price))
            .ToList();
    }

    public async Task<IReadOnlyList<ProductContextItem>> SearchProductsAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetCatalogAsync(cancellationToken);

        var baseUrl = options.Value.ProductQueriesBaseUrl.TrimEnd('/');
        var encoded = Uri.EscapeDataString(query.Trim());
        var products = await httpClient.GetFromJsonAsync<List<ProductSearchApiDto>>(
            $"{baseUrl}/api/products/search?q={encoded}&size=8",
            cancellationToken);

        return (products ?? [])
            .Select(p => new ProductContextItem(p.Id, p.Name, p.Price))
            .ToList();
    }
}

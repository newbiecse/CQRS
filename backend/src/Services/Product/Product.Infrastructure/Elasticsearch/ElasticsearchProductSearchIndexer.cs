using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Product.Application.Abstractions;
using Product.Application.ReadModels;
using Product.Infrastructure.Options;

namespace Product.Infrastructure.Elasticsearch;

public sealed class ElasticsearchProductSearchIndexer(
    ElasticsearchClient client,
    IOptions<ProductElasticsearchOptions> options,
    ILogger<ElasticsearchProductSearchIndexer> logger) : IProductSearchIndexer
{
    public async Task IndexAsync(ProductReadModel product, CancellationToken cancellationToken = default)
    {
        var indexName = options.Value.ProductIndex;
        var document = new ProductSearchDocument
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            CreatedAt = product.CreatedAt,
            LastUpdatedAt = product.LastUpdatedAt
        };

        var response = await client.IndexAsync(document, indexName, product.Id.ToString(), cancellationToken);
        if (!response.IsValidResponse)
        {
            logger.LogWarning(
                "Failed to index product {ProductId}: {Error}",
                product.Id,
                response.ElasticsearchServerError?.Error.Reason ?? response.DebugInformation);
        }
    }

    public async Task DeleteAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var indexName = options.Value.ProductIndex;
        var response = await client.DeleteAsync(indexName, Id.From(productId), cancellationToken);
        if (!response.IsValidResponse)
        {
            logger.LogWarning(
                "Failed to delete product {ProductId} from index: {Error}",
                productId,
                response.ElasticsearchServerError?.Error.Reason ?? response.DebugInformation);
        }
    }
}

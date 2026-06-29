using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Product.Application.Abstractions;
using Product.Application.ReadModels;
using Product.Infrastructure.Options;

namespace Product.Infrastructure.Elasticsearch;

public sealed class ElasticsearchProductSearchReader(
    ElasticsearchClient client,
    IOptions<ProductElasticsearchOptions> options,
    ILogger<ElasticsearchProductSearchReader> logger) : IProductSearchReader
{
    public async Task<IReadOnlyList<ProductSearchResult>> SearchAsync(
        string query,
        int size,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var indexName = options.Value.ProductIndex;
        var take = Math.Clamp(size, 1, 100);

        var response = await client.SearchAsync<ProductSearchDocument>(indexName, s => s
            .Query(q => q.Match(new MatchQuery((Field)"name")
            {
                Query = query.Trim(),
                Fuzziness = new Fuzziness("AUTO")
            }))
            .Highlight(h => h
                .Fields(f => f
                    .Add((Field)"name", hf => hf
                        .PreTags(["<mark>"])
                        .PostTags(["</mark>"])
                        .NumberOfFragments(0))))
            .Size(take)
            .Sort(sort => sort.Score(new ScoreSort { Order = SortOrder.Desc })),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            logger.LogError(
                "Product search failed: {Error}",
                response.ElasticsearchServerError?.Error.Reason ?? response.DebugInformation);
            throw new InvalidOperationException("Product search is temporarily unavailable.");
        }

        return response.Hits
            .Where(hit => hit.Source is not null)
            .Select(MapHit)
            .ToList();
    }

    private static ProductSearchResult MapHit(Hit<ProductSearchDocument> hit)
    {
        var source = hit.Source!;
        var highlightedName = hit.Highlight?.TryGetValue("name", out var fragments) == true
            && fragments.Count > 0
            ? fragments.First()
            : source.Name;

        return new ProductSearchResult
        {
            Id = source.Id,
            Name = source.Name,
            HighlightedName = highlightedName,
            Price = source.Price,
            CreatedAt = source.CreatedAt,
            LastUpdatedAt = source.LastUpdatedAt
        };
    }
}

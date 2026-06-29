using Audit.Infrastructure.Elasticsearch;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Audit.Application.Abstractions;
using Audit.Application.Models;
using Audit.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Audit.Infrastructure.Elasticsearch;

public sealed class ElasticsearchBusinessAuditReader(
    ElasticsearchClient client,
    IOptions<ElasticsearchOptions> options,
    ILogger<ElasticsearchBusinessAuditReader> logger) : IBusinessAuditReader
{
    public async Task<IReadOnlyList<BusinessAuditRecord>> SearchAsync(
        BusinessAuditSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var indexName = options.Value.BusinessAuditIndex;
        var filters = new List<Query>();

        if (!string.IsNullOrWhiteSpace(query.EntityType))
            filters.Add(Query.Term(new TermQuery((Field)"entityType") { Value = query.EntityType }));

        if (!string.IsNullOrWhiteSpace(query.EntityId))
            filters.Add(Query.Term(new TermQuery((Field)"entityId") { Value = query.EntityId }));

        if (!string.IsNullOrWhiteSpace(query.EventType))
            filters.Add(Query.Term(new TermQuery((Field)"eventType") { Value = query.EventType }));

        Query searchQuery = filters.Count switch
        {
            0 when string.IsNullOrWhiteSpace(query.SearchText) => Query.MatchAll(new MatchAllQuery()),
            0 => Query.Match(new MatchQuery((Field)"summary") { Query = query.SearchText }),
            _ when string.IsNullOrWhiteSpace(query.SearchText) => Query.Bool(new BoolQuery { Filter = filters }),
            _ => Query.Bool(new BoolQuery
            {
                Filter = filters,
                Must = [Query.Match(new MatchQuery((Field)"summary") { Query = query.SearchText })]
            })
        };

        var response = await client.SearchAsync<BusinessAuditRecord>(indexName, s => s
            .Query(searchQuery)
            .Sort(sort => sort.Field((Field)"occurredAtUtc", new FieldSort { Order = SortOrder.Desc }))
            .Size(Math.Clamp(query.Size, 1, 200)),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            logger.LogError(
                "Business audit search failed: {Error}",
                response.ElasticsearchServerError?.Error.Reason ?? response.DebugInformation);
            throw new InvalidOperationException("Failed to search business audit records.");
        }

        return response.Documents.ToList();
    }
}

using Audit.Application.Abstractions;
using Audit.Application.Models;
using Audit.Infrastructure.Options;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Audit.Infrastructure.Elasticsearch;

public sealed class ElasticsearchBusinessAuditWriter(
    ElasticsearchClient client,
    IOptions<ElasticsearchOptions> options,
    ILogger<ElasticsearchBusinessAuditWriter> logger) : IBusinessAuditWriter
{
    public async Task IndexAsync(BusinessAuditRecord record, CancellationToken cancellationToken = default)
    {
        var indexName = options.Value.BusinessAuditIndex;
        var response = await client.IndexAsync(
            record,
            indexName,
            record.Id,
            cancellationToken);

        if (!response.IsValidResponse)
        {
            logger.LogError(
                "Failed to index business audit {AuditId}: {Error}",
                record.Id,
                response.ElasticsearchServerError?.Error.Reason ?? response.DebugInformation);
            throw new InvalidOperationException("Failed to index business audit record.");
        }
    }
}

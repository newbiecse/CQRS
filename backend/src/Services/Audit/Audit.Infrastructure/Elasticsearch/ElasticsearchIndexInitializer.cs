using Audit.Application.Abstractions;
using Audit.Application.Models;
using Audit.Infrastructure.Options;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Audit.Infrastructure.Elasticsearch;

public sealed class ElasticsearchIndexInitializer(
    ElasticsearchClient client,
    IOptions<ElasticsearchOptions> options,
    ILogger<ElasticsearchIndexInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var indexName = options.Value.BusinessAuditIndex;
        var exists = await client.Indices.ExistsAsync(indexName, cancellationToken);
        if (exists.Exists)
            return;

        var response = await client.Indices.CreateAsync(indexName, c => c
            .Mappings(m => m.Properties(CreateMappings())),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            logger.LogWarning(
                "Could not create Elasticsearch index {Index}: {Error}",
                indexName,
                response.ElasticsearchServerError?.Error.Reason ?? response.DebugInformation);
            return;
        }

        logger.LogInformation("Created Elasticsearch index {Index}", indexName);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static Properties CreateMappings() =>
        new Properties
        {
            { "occurredAtUtc", new DateProperty() },
            { "eventType", new KeywordProperty() },
            { "entityType", new KeywordProperty() },
            { "entityId", new KeywordProperty() },
            { "action", new KeywordProperty() },
            { "service", new KeywordProperty() },
            { "summary", new TextProperty() },
            { "payloadJson", new TextProperty { Index = false } }
        };
}

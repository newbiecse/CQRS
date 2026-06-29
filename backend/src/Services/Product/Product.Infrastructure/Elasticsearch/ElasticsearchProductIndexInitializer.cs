using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Product.Infrastructure.Options;

namespace Product.Infrastructure.Elasticsearch;

public sealed class ElasticsearchProductIndexInitializer(
    ElasticsearchClient client,
    IOptions<ProductElasticsearchOptions> options,
    ILogger<ElasticsearchProductIndexInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var indexName = options.Value.ProductIndex;
        var exists = await client.Indices.ExistsAsync(indexName, cancellationToken);
        if (exists.Exists)
            return;

        var response = await client.Indices.CreateAsync(indexName, c => c
            .Mappings(m => m.Properties(new Properties
            {
                { "id", new KeywordProperty() },
                { "name", new TextProperty() },
                { "price", new DoubleNumberProperty() },
                { "createdAt", new DateProperty() },
                { "lastUpdatedAt", new DateProperty() }
            })),
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
}

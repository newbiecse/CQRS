using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Product.Application.Abstractions;

namespace Product.Infrastructure.Elasticsearch;

public sealed class ProductSearchReindexHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<ProductSearchReindexHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var readRepository = scope.ServiceProvider.GetRequiredService<IProductReadRepository>();
        var indexer = scope.ServiceProvider.GetRequiredService<IProductSearchIndexer>();

        var products = await readRepository.GetAllAsync(cancellationToken);
        foreach (var product in products)
            await indexer.IndexAsync(product, cancellationToken);

        logger.LogInformation("Indexed {Count} products into Elasticsearch", products.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

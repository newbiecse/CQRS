using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Application.Abstractions;
using Product.Infrastructure.Elasticsearch;
using Product.Infrastructure.Integration;
using Product.Infrastructure.Options;
using Product.Infrastructure.Persistence.Read;
using Product.Infrastructure.Persistence.Write;
using Product.Infrastructure.Projections;

namespace Product.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProductWriteInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWriteDbContext<ProductWriteDbContext>(configuration);
        services.AddScoped<IIntegrationEventMapper, ProductIntegrationEventMapper>();
        services.AddScoped<IProductWriteRepository, SqlProductWriteRepository>();
        return services;
    }

    public static IServiceCollection AddProductReadInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("ReadDb")
            ?? throw new InvalidOperationException("ReadDb connection missing.");
        services.AddDbContext<ProductReadDbContext>(o => o.UseSqlServer(conn));
        services.AddScoped<IProductReadRepository, SqlProductReadRepository>();
        services.AddScoped<ProductProjectionHandler>();
        return services;
    }

    public static IServiceCollection AddProductElasticsearchSearch(
        this IServiceCollection services,
        IConfiguration configuration,
        bool enableReindexOnStartup = false)
    {
        services.Configure<ProductElasticsearchOptions>(configuration.GetSection(ProductElasticsearchOptions.SectionName));

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ProductElasticsearchOptions>>().Value;
            var settings = new ElasticsearchClientSettings(new Uri(options.Url))
                .DefaultMappingFor<ProductSearchDocument>(m => m.IndexName(options.ProductIndex));
            return new ElasticsearchClient(settings);
        });

        services.AddSingleton<IProductSearchIndexer, ElasticsearchProductSearchIndexer>();
        services.AddSingleton<IProductSearchReader, ElasticsearchProductSearchReader>();
        services.AddHostedService<ElasticsearchProductIndexInitializer>();

        if (enableReindexOnStartup)
            services.AddHostedService<ProductSearchReindexHostedService>();

        return services;
    }

    public static Task InitializeProductWriteStoreAsync(this IServiceProvider sp) =>
        sp.InitializeWriteStoreAsync<ProductWriteDbContext>();

    public static async Task InitializeProductReadStoreAsync(this IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ProductReadDbContext>().Database.EnsureCreatedAsync();
    }
}

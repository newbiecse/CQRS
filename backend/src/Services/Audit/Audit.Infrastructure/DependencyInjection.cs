using Audit.Application;
using Audit.Application.Abstractions;
using Audit.Infrastructure.Elasticsearch;
using Audit.Infrastructure.Options;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditElasticsearch(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ElasticsearchOptions>(configuration.GetSection(ElasticsearchOptions.SectionName));
        services.AddAuditApplication();

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ElasticsearchOptions>>().Value;
            var settings = new ElasticsearchClientSettings(new Uri(options.Url))
                .DefaultMappingFor<Audit.Application.Models.BusinessAuditRecord>(m => m
                    .IndexName(options.BusinessAuditIndex));
            return new ElasticsearchClient(settings);
        });

        services.AddSingleton<IBusinessAuditWriter, ElasticsearchBusinessAuditWriter>();
        services.AddSingleton<IBusinessAuditReader, ElasticsearchBusinessAuditReader>();
        services.AddHostedService<ElasticsearchIndexInitializer>();

        return services;
    }
}

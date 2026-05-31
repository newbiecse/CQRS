using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.BuildingBlocks.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsDemo.BuildingBlocks.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddServiceBusPublisher(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureServiceBusOptions>(configuration.GetSection(AzureServiceBusOptions.SectionName));
        services.AddSingleton<IIntegrationEventPublisher, AzureServiceBusIntegrationEventPublisher>();
        return services;
    }

    public static IServiceCollection AddServiceBusOutbox(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddServiceBusPublisher(configuration);
        services.AddHostedService<OutboxPublisherBackgroundService>();
        return services;
    }
}

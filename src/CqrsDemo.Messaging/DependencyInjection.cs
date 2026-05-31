using CqrsDemo.Messaging.Abstractions;
using CqrsDemo.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsDemo.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddAzureServiceBusMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AzureServiceBusOptions>(
            configuration.GetSection(AzureServiceBusOptions.SectionName));

        services.AddSingleton<IIntegrationEventPublisher, AzureServiceBusIntegrationEventPublisher>();
        services.AddSingleton<ServiceBusTopologyInitializer>();
        services.AddHostedService<ServiceBusTopologyHostedService>();

        return services;
    }
}

using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Commands.Infrastructure.Integration;
using CqrsDemo.Commands.Infrastructure.Persistence.Write;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsDemo.Commands.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCommandsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddWriteDbContext<WriteDbContext>(configuration);
        services.AddScoped<IIntegrationEventMapper, MonolithIntegrationEventMapper>();
        services.AddScoped<IProductWriteRepository, SqlProductWriteRepository>();
        services.AddScoped<ICartWriteRepository, SqlCartWriteRepository>();
        services.AddScoped<IOrderWriteRepository, SqlOrderWriteRepository>();
        services.AddScoped<IPaymentWriteRepository, SqlPaymentWriteRepository>();

        return services;
    }

    public static Task MigrateCommandsDatabaseAsync(this IServiceProvider serviceProvider) =>
        serviceProvider.InitializeWriteStoreAsync<WriteDbContext>();
}

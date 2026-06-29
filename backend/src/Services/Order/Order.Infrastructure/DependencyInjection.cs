using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Abstractions;
using Order.Application.Integration;
using Order.Infrastructure.Integration;
using Order.Infrastructure.Persistence.Read;
using Order.Infrastructure.Persistence.Write;
using Order.Infrastructure.Projections;

namespace Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderWriteInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWriteDbContext<OrderWriteDbContext>(configuration);
        services.AddScoped<IIntegrationEventMapper, OrderIntegrationEventMapper>();
        services.AddScoped<IOrderWriteRepository, SqlOrderWriteRepository>();
        services.AddScoped<OrderIntegrationHandlers>();
        return services;
    }

    public static IServiceCollection AddOrderReadInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var readConnection = configuration.GetConnectionString("ReadDb")
            ?? throw new InvalidOperationException("Connection string 'ReadDb' is not configured.");
        services.AddDbContext<OrderReadDbContext>(o => o.UseSqlServer(readConnection));
        services.AddScoped<IOrderReadRepository, SqlOrderReadRepository>();
        services.AddScoped<OrderProjectionHandler>();
        return services;
    }

    public static Task InitializeOrderWriteStoreAsync(this IServiceProvider serviceProvider) =>
        serviceProvider.InitializeWriteStoreAsync<OrderWriteDbContext>();

    public static async Task InitializeOrderReadStoreAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<OrderReadDbContext>().Database.EnsureCreatedAsync();
    }
}

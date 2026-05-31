using CqrsDemo.BuildingBlocks.EventStore;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.EventStore.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Abstractions;
using Order.Application.Integration;
using Order.Infrastructure.EventStore;
using Order.Infrastructure.Persistence.Read;
using Order.Infrastructure.Projections;

namespace Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderWriteInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEventStoreInfrastructure(configuration, "WriteDb");
        services.AddScoped<IDomainEventSerializer, OrderDomainEventSerializer>();
        services.AddScoped<IIntegrationEventMapper, OrderIntegrationEventMapper>();
        services.AddServiceBusOutbox(configuration);
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

    public static async Task InitializeOrderWriteStoreAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<EventStoreDbContext>().Database.EnsureCreatedAsync();
    }

    public static async Task InitializeOrderReadStoreAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<OrderReadDbContext>().Database.EnsureCreatedAsync();
    }
}

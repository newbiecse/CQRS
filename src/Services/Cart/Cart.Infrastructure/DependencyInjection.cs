using Cart.Application.Abstractions;
using Cart.Infrastructure.EventStore;
using Cart.Infrastructure.Persistence.Read;
using Cart.Infrastructure.Projections;
using CqrsDemo.BuildingBlocks.EventStore;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.EventStore.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCartWriteInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEventStoreInfrastructure(configuration, "WriteDb");
        services.AddScoped<IDomainEventSerializer, CartDomainEventSerializer>();
        services.AddScoped<IIntegrationEventMapper, CartIntegrationEventMapper>();
        services.AddServiceBusOutbox(configuration);
        return services;
    }

    public static IServiceCollection AddCartReadInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var readConnection = configuration.GetConnectionString("ReadDb")
            ?? throw new InvalidOperationException("Connection string 'ReadDb' is not configured.");
        services.AddDbContext<CartReadDbContext>(o => o.UseSqlServer(readConnection));
        services.AddScoped<ICartReadRepository, SqlCartReadRepository>();
        services.AddScoped<CartProjectionHandler>();
        return services;
    }

    public static async Task InitializeCartWriteStoreAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<EventStoreDbContext>().Database.EnsureCreatedAsync();
    }

    public static async Task InitializeCartReadStoreAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CartReadDbContext>().Database.EnsureCreatedAsync();
    }
}

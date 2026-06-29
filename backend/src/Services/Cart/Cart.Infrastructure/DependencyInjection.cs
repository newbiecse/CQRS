using Cart.Application.Abstractions;
using Cart.Infrastructure.Integration;
using Cart.Infrastructure.Persistence.Read;
using Cart.Infrastructure.Persistence.Write;
using Cart.Infrastructure.Projections;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCartWriteInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWriteDbContext<CartWriteDbContext>(configuration);
        services.AddScoped<IIntegrationEventMapper, CartIntegrationEventMapper>();
        services.AddScoped<ICartWriteRepository, SqlCartWriteRepository>();
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

    public static Task InitializeCartWriteStoreAsync(this IServiceProvider serviceProvider) =>
        serviceProvider.InitializeWriteStoreAsync<CartWriteDbContext>();

    public static async Task InitializeCartReadStoreAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CartReadDbContext>().Database.EnsureCreatedAsync();
    }
}

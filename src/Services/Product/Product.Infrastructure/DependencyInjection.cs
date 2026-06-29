using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Application.Abstractions;
using Product.Infrastructure.Integration;
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

    public static Task InitializeProductWriteStoreAsync(this IServiceProvider sp) =>
        sp.InitializeWriteStoreAsync<ProductWriteDbContext>();

    public static async Task InitializeProductReadStoreAsync(this IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ProductReadDbContext>().Database.EnsureCreatedAsync();
    }
}

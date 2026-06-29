using Inventory.Application.Abstractions;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Inventory.Application.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("InventoryDb")
            ?? configuration.GetConnectionString("WriteDb")
            ?? throw new InvalidOperationException("Connection string 'InventoryDb' or 'WriteDb' is not configured.");

        services.AddDbContext<InventoryDbContext>(o => o.UseSqlServer(connection));
        services.AddScoped<IInventoryRepository, SqlInventoryRepository>();
        services.AddScoped<InventoryIntegrationHandlers>();
        return services;
    }

    public static async Task InitializeInventoryStoreAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.EnsureCreatedAsync();
    }
}

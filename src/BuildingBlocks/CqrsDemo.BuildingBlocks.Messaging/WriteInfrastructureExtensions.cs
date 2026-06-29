using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsDemo.BuildingBlocks.Messaging;

public static class WriteInfrastructureExtensions
{
    public static IServiceCollection AddWriteDbContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionName = "WriteDb")
        where TContext : DbContext, IOutboxDbContext
    {
        var connection = configuration.GetConnectionString(connectionName)
            ?? throw new InvalidOperationException($"Connection string '{connectionName}' is not configured.");

        services.AddDbContext<TContext>(options => options.UseSqlServer(connection));
        services.AddScoped<IOutboxDbContext>(sp => sp.GetRequiredService<TContext>());
        services.AddServiceBusOutbox(configuration);
        return services;
    }

    public static async Task InitializeWriteStoreAsync<TContext>(this IServiceProvider serviceProvider)
        where TContext : DbContext, IOutboxDbContext
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<TContext>().Database.EnsureCreatedAsync();
    }
}

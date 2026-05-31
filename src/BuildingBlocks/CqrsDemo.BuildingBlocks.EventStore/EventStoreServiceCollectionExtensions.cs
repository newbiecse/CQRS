using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using CqrsDemo.BuildingBlocks.EventStore.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsDemo.BuildingBlocks.EventStore;

public static class EventStoreServiceCollectionExtensions
{
    public static IServiceCollection AddEventStoreInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string writeConnectionName = "WriteDb")
    {
        var connection = configuration.GetConnectionString(writeConnectionName)
            ?? throw new InvalidOperationException($"Connection string '{writeConnectionName}' missing.");

        services.AddDbContext<EventStoreDbContext>(o => o.UseSqlServer(connection));
        services.AddScoped<IEventStore, SqlEventStore>();
        return services;
    }

    public static async Task InitializeEventStoreAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<EventStoreDbContext>().Database.EnsureCreatedAsync();
    }
}

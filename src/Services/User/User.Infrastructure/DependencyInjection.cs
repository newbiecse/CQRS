using CqrsDemo.BuildingBlocks.EventStore;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using CqrsDemo.BuildingBlocks.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using User.Application.Abstractions;
using User.Infrastructure.EventStore;
using User.Infrastructure.Persistence.Read;
using User.Infrastructure.Projections;

namespace User.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserWriteInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEventStoreInfrastructure(configuration, "WriteDb");
        services.AddScoped<IDomainEventSerializer, UserDomainEventSerializer>();
        services.AddScoped<IIntegrationEventMapper, UserIntegrationEventMapper>();
        services.AddServiceBusOutbox(configuration);
        return services;
    }

    public static IServiceCollection AddUserReadInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("ReadDb")
            ?? throw new InvalidOperationException("Connection string 'ReadDb' is not configured.");
        services.AddDbContext<UserReadDbContext>(o => o.UseSqlServer(conn));
        services.AddScoped<IUserReadRepository, SqlUserReadRepository>();
        services.AddScoped<UserProjectionHandler>();
        return services;
    }

    public static Task InitializeUserWriteStoreAsync(this IServiceProvider sp) =>
        sp.InitializeEventStoreAsync();

    public static async Task InitializeUserReadStoreAsync(this IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        await scope.ServiceProvider.GetRequiredService<UserReadDbContext>().Database.EnsureCreatedAsync();
    }
}

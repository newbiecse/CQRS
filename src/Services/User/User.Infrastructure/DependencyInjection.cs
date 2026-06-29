using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using User.Application.Abstractions;
using User.Infrastructure.Integration;
using User.Infrastructure.Persistence.Read;
using User.Infrastructure.Persistence.Write;
using User.Infrastructure.Projections;

namespace User.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserWriteInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWriteDbContext<UserWriteDbContext>(configuration);
        services.AddScoped<IIntegrationEventMapper, UserIntegrationEventMapper>();
        services.AddScoped<IUserWriteRepository, SqlUserWriteRepository>();
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
        sp.InitializeWriteStoreAsync<UserWriteDbContext>();

    public static async Task InitializeUserReadStoreAsync(this IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        await scope.ServiceProvider.GetRequiredService<UserReadDbContext>().Database.EnsureCreatedAsync();
    }
}

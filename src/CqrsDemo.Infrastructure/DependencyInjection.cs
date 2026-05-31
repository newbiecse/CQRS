using CqrsDemo.Application.Abstractions;
using CqrsDemo.Infrastructure.Events;
using CqrsDemo.Infrastructure.Persistence;
using CqrsDemo.Infrastructure.Persistence.Read;
using CqrsDemo.Infrastructure.Persistence.Write;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsDemo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var writeConnection = configuration.GetConnectionString("WriteDb")
            ?? throw new InvalidOperationException("Connection string 'WriteDb' is not configured.");

        var readConnection = configuration.GetConnectionString("ReadDb")
            ?? throw new InvalidOperationException("Connection string 'ReadDb' is not configured.");

        services.AddDbContext<WriteDbContext>(options =>
            options.UseSqlServer(writeConnection));

        services.AddDbContext<ReadDbContext>(options =>
            options.UseSqlServer(readConnection));

        services.AddScoped<IProductWriteRepository, SqlProductWriteRepository>();
        services.AddScoped<IProductReadRepository, SqlProductReadRepository>();
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

        return services;
    }

    public static async Task MigrateDatabasesAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var writeDb = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        var readDb = scope.ServiceProvider.GetRequiredService<ReadDbContext>();

        await writeDb.Database.MigrateAsync();
        await readDb.Database.MigrateAsync();
    }
}

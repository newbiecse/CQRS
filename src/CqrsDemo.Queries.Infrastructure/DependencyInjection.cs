using CqrsDemo.Queries.Application.Abstractions;
using CqrsDemo.Queries.Infrastructure.Persistence;
using CqrsDemo.Queries.Infrastructure.Persistence.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsDemo.Queries.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddQueriesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var readConnection = configuration.GetConnectionString("ReadDb")
            ?? throw new InvalidOperationException("Connection string 'ReadDb' is not configured.");

        services.AddDbContext<ReadDbContext>(options =>
            options.UseSqlServer(readConnection));

        services.AddScoped<IProductReadRepository, SqlProductReadRepository>();
        services.AddScoped<ICartReadRepository, SqlCartReadRepository>();
        services.AddScoped<IOrderReadRepository, SqlOrderReadRepository>();
        services.AddScoped<IPaymentReadRepository, SqlPaymentReadRepository>();

        return services;
    }

    public static async Task MigrateQueriesDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}

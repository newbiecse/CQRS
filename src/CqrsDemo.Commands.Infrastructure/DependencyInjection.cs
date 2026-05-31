using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Commands.Infrastructure.Outbox;
using CqrsDemo.Commands.Infrastructure.Persistence.EventStore;
using CqrsDemo.Commands.Infrastructure.Persistence.Write;
using CqrsDemo.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsDemo.Commands.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCommandsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var writeConnection = configuration.GetConnectionString("WriteDb")
            ?? throw new InvalidOperationException("Connection string 'WriteDb' is not configured.");

        services.AddDbContext<WriteDbContext>(options =>
            options.UseSqlServer(writeConnection));

        services.AddScoped<IEventStore, SqlEventStore>();
        services.AddScoped<IProductWriteUnitOfWork, EventSourcedProductRepository>();
        services.AddAzureServiceBusMessaging(configuration);
        services.AddHostedService<OutboxPublisherBackgroundService>();

        return services;
    }

    public static async Task MigrateCommandsDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}

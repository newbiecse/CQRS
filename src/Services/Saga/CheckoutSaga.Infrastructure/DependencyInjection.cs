using CheckoutSaga.Application.Abstractions;
using CheckoutSaga.Infrastructure.Http;
using CheckoutSaga.Infrastructure.Messaging;
using CheckoutSaga.Infrastructure.Persistence;
using CqrsDemo.BuildingBlocks.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckoutSaga.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCheckoutSagaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("SagaDb")
            ?? throw new InvalidOperationException("Connection string 'SagaDb' is not configured.");

        services.AddDbContext<CheckoutSagaDbContext>(o => o.UseSqlServer(connection));
        services.AddScoped<ICheckoutSagaRepository, SqlCheckoutSagaRepository>();
        services.AddShopServiceClients(configuration);
        services.AddServiceBusPublisher(configuration);
        services.AddSingleton<ICheckoutSagaNotifier, CheckoutSagaNotifier>();
        return services;
    }

    public static async Task InitializeCheckoutSagaStoreAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CheckoutSagaDbContext>().Database.EnsureCreatedAsync();
    }
}

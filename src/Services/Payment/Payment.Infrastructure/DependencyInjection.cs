using CqrsDemo.BuildingBlocks.EventStore;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.EventStore.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Abstractions;
using Payment.Infrastructure.EventStore;
using Payment.Infrastructure.Http;
using Payment.Infrastructure.Persistence.Read;
using Payment.Infrastructure.Projections;

namespace Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentWriteInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEventStoreInfrastructure(configuration, "WriteDb");
        services.AddScoped<IDomainEventSerializer, PaymentDomainEventSerializer>();
        services.AddScoped<IIntegrationEventMapper, PaymentIntegrationEventMapper>();
        services.AddServiceBusOutbox(configuration);
        services.AddOrderServiceClient(configuration);
        return services;
    }

    public static IServiceCollection AddPaymentReadInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var readConnection = configuration.GetConnectionString("ReadDb")
            ?? throw new InvalidOperationException("Connection string 'ReadDb' is not configured.");
        services.AddDbContext<PaymentReadDbContext>(o => o.UseSqlServer(readConnection));
        services.AddScoped<IPaymentReadRepository, SqlPaymentReadRepository>();
        services.AddScoped<PaymentProjectionHandler>();
        return services;
    }

    public static async Task InitializePaymentWriteStoreAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<EventStoreDbContext>().Database.EnsureCreatedAsync();
    }

    public static async Task InitializePaymentReadStoreAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PaymentReadDbContext>().Database.EnsureCreatedAsync();
    }
}

using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Abstractions;
using Payment.Infrastructure.Http;
using Payment.Infrastructure.Integration;
using Payment.Infrastructure.Persistence.Read;
using Payment.Infrastructure.Persistence.Write;
using Payment.Infrastructure.Projections;

namespace Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentWriteInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWriteDbContext<PaymentWriteDbContext>(configuration);
        services.AddScoped<IIntegrationEventMapper, PaymentIntegrationEventMapper>();
        services.AddScoped<IPaymentWriteRepository, SqlPaymentWriteRepository>();
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

    public static Task InitializePaymentWriteStoreAsync(this IServiceProvider serviceProvider) =>
        serviceProvider.InitializeWriteStoreAsync<PaymentWriteDbContext>();

    public static async Task InitializePaymentReadStoreAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PaymentReadDbContext>().Database.EnsureCreatedAsync();
    }
}

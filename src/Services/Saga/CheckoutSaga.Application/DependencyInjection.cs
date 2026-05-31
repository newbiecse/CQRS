using System.Reflection;
using CheckoutSaga.Application.Orchestration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckoutSaga.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCheckoutSagaApplication(this IServiceCollection services)
    {
        services.AddMediatR(c => c.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddScoped<CheckoutSagaOrchestrator>();
        return services;
    }
}

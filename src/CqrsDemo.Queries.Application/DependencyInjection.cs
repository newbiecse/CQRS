using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsDemo.Queries.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddQueriesApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        return services;
    }
}

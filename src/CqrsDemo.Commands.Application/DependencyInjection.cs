using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsDemo.Commands.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCommandsApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        return services;
    }
}

using System.Reflection;
using Auth.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthApplication(this IServiceCollection services)
    {
        services.AddMediatR(c => c.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddSingleton<IAuthTokenIssuer, AuthTokenIssuer>();
        return services;
    }
}

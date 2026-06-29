using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Cart.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCartApplication(this IServiceCollection services) =>
        services.AddMediatR(c => c.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
}

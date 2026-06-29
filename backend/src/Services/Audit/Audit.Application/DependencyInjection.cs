using Audit.Application.Projections;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditApplication(this IServiceCollection services)
    {
        services.AddSingleton<BusinessAuditProjectionHandler>();
        return services;
    }
}

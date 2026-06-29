using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reporting.Application.Abstractions;
using Reporting.Infrastructure.Persistence;
using Reporting.Infrastructure.Projections;

namespace Reporting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("ReportingDb")
            ?? throw new InvalidOperationException("Connection string 'ReportingDb' is not configured.");

        services.AddDbContext<ReportingDbContext>(o => o.UseSqlServer(connection));
        services.AddScoped<IReportingWriteRepository, SqlReportingWriteRepository>();
        services.AddScoped<IReportingReadRepository, SqlReportingReadRepository>();
        services.AddScoped<ReportingProjectionHandler>();
        return services;
    }

    public static async Task InitializeReportingStoreAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ReportingDbContext>().Database.EnsureCreatedAsync();
    }
}

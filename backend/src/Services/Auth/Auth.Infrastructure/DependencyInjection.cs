using Auth.Application.Abstractions;
using Auth.Application.Admin;
using Auth.Application.Commands.RegisterLocalUser;
using Auth.Infrastructure.Http;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Security;
using CqrsDemo.BuildingBlocks.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("IdentityDb")
            ?? throw new InvalidOperationException("Connection string 'IdentityDb' is not configured.");

        services.AddDbContext<IdentityDbContext>(o => o.UseSqlServer(connection));
        services.AddScoped<IIdentityRepository, SqlIdentityRepository>();
        services.AddScoped<IAuthorizationStore, SqlAuthorizationStore>();
        services.AddScoped<IAuthorizationAdminService, AuthorizationAdminService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddUserProfileProvisioner(configuration);
        return services;
    }

    public static async Task InitializeIdentityStoreAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureCreatedAsync();
    }
}

public static class AdminIdentitySeeder
{
    public const string DefaultAdminEmail = "admin@cqrs.local";
    public const string DefaultAdminPassword = "Admin123!";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IIdentityRepository>();
        if (await repository.ExistsByEmailAsync(DefaultAdminEmail, cancellationToken))
            return;

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(
            new RegisterLocalUserCommand(
                DefaultAdminEmail,
                "Platform Admin",
                DefaultAdminPassword,
                [PlatformRoles.Admin]),
            cancellationToken);
    }
}

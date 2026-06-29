using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CqrsDemo.BuildingBlocks.Auth;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddPlatformJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(PlatformPolicies.Authenticated, policy => policy.RequireAuthenticatedUser())
            .AddPolicy(PlatformPolicies.AdminOnly, policy =>
                policy.RequireAuthenticatedUser().RequireRole(PlatformRoles.Admin))
            .AddPolicy(PlatformPolicies.CanManageCatalog, policy =>
                policy.RequireAuthenticatedUser().RequireRole(
                    PlatformRoles.Admin,
                    PlatformRoles.CatalogManager))
            .AddPolicy(PlatformPolicies.CanManageOrders, policy =>
                policy.RequireAuthenticatedUser().RequireRole(
                    PlatformRoles.Admin,
                    PlatformRoles.OrderManager));

        return services;
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using CqrsDemo.BuildingBlocks.Auth;
using Microsoft.Extensions.Options;
using Shop.Admin.Api.Clients;
using Shop.Admin.Api.Options;

namespace Shop.Admin.Api.Endpoints;

internal static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/login", LoginAsync);
        group.MapPost("/register", RegisterAsync);
        group.MapGet("/google", OAuthRedirectAsync("google"));
        group.MapGet("/facebook", OAuthRedirectAsync("facebook"));
        group.MapGet("/me", GetCurrentUserAsync).RequireAuthorization(PlatformPolicies.Authenticated);

        app.MapPost("/api/login/account", LoginAsync);
        app.MapPost("/api/login/outLogin", () => Results.Ok(new { data = new { }, success = true }));
        app.MapGet("/api/currentUser", GetCurrentUserAsync).RequireAuthorization(PlatformPolicies.Authenticated);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        AdminBackendClient client,
        IOptions<AdminShopServiceOptions> options,
        CancellationToken cancellationToken)
    {
        var email = (request.Email ?? request.Username ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(email))
            return Results.BadRequest(new { message = "Email is required." });

        var response = await client.SendRawAsync(
            HttpMethod.Post,
            options.Value.AuthApi,
            "/api/auth/login",
            new { email, password = request.Password ?? string.Empty },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Results.Unauthorized();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return Results.Content(json, "application/json");
    }

    private static async Task<IResult> RegisterAsync(
        RegisterAuthRequest request,
        AdminBackendClient client,
        IOptions<AdminShopServiceOptions> options,
        CancellationToken cancellationToken)
    {
        var response = await client.SendRawAsync(
            HttpMethod.Post,
            options.Value.AuthApi,
            "/api/auth/register",
            request,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return Results.Content(body, "application/json", statusCode: (int)response.StatusCode);
    }

    private static Func<IOptions<AdminShopServiceOptions>, IResult> OAuthRedirectAsync(string provider) =>
        options => Results.Redirect($"{options.Value.AuthApi.TrimEnd('/')}/api/auth/{provider}");

    private static IResult GetCurrentUserAsync(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
            return Results.Unauthorized();

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");
        var email = user.FindFirstValue(ClaimTypes.Email)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Email);
        var name = user.FindFirstValue(ClaimTypes.Name) ?? email;
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        var primaryRole = roles.FirstOrDefault() ?? PlatformRoles.Admin;

        return Results.Ok(new
        {
            success = true,
            data = new
            {
                name,
                userid = userId,
                email,
                access = primaryRole,
                roles,
                avatar = "https://gw.alipayobjects.com/zos/antfincdn/XAosXuNZyF/BiazfanxmamNRoxxVxka.png"
            }
        });
    }
}

internal sealed record LoginRequest(
    string? Username = null,
    string? Email = null,
    string? Password = null,
    string? Type = null);

internal sealed record RegisterAuthRequest(
    string Email,
    string DisplayName,
    string Password,
    IReadOnlyList<string>? Roles = null);

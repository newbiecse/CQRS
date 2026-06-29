using System.Security.Claims;
using Auth.Api.Endpoints;
using Auth.Application;
using Auth.Application.Commands.HandleExternalLogin;
using Auth.Application.Commands.LoginWithPassword;
using Auth.Application.Commands.RegisterLocalUser;
using Auth.Domain;
using Auth.Infrastructure;
using Auth.Infrastructure.Options;
using CqrsDemo.BuildingBlocks.Auth;
using CqrsDemo.BuildingBlocks.Observability;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Options;

const string ExternalScheme = "External";

var builder = WebApplication.CreateBuilder(args);
builder.AddPlatformObservability("auth-api");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<OAuthOptions>(builder.Configuration.GetSection(OAuthOptions.SectionName));
builder.Services.AddAuthApplication();
builder.Services.AddAuthInfrastructure(builder.Configuration);
builder.Services.AddPlatformJwtAuthentication(builder.Configuration);

var oauth = builder.Configuration.GetSection(OAuthOptions.SectionName).Get<OAuthOptions>() ?? new OAuthOptions();
var authBuilder = builder.Services.AddAuthentication();

authBuilder.AddCookie(ExternalScheme, _ => { });

if (!string.IsNullOrWhiteSpace(oauth.Google.ClientId))
{
    authBuilder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.SignInScheme = ExternalScheme;
        options.ClientId = oauth.Google.ClientId;
        options.ClientSecret = oauth.Google.ClientSecret;
    });
}

if (!string.IsNullOrWhiteSpace(oauth.Facebook.AppId))
{
    authBuilder.AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
    {
        options.SignInScheme = ExternalScheme;
        options.AppId = oauth.Facebook.AppId;
        options.AppSecret = oauth.Facebook.AppSecret;
    });
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:8000", "http://localhost:5100", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();
app.UsePlatformObservability();
await app.Services.InitializeIdentityStoreAsync();
await RbacSeeder.SeedAsync(app.Services);
await AdminIdentitySeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    service = "Auth.Api",
    port = 5207,
    providers = new[] { "local", "google", "facebook" }
}));

app.MapPost("/api/auth/login", async (LoginRequest request, IMediator mediator) =>
{
    var result = await mediator.Send(new LoginWithPasswordCommand(request.Email, request.Password));
    return result is null ? Results.Unauthorized() : Results.Ok(ToLoginResponse(result));
});

app.MapPost("/api/auth/register", async (RegisterRequest request, IMediator mediator) =>
{
    try
    {
        var id = await mediator.Send(new RegisterLocalUserCommand(
            request.Email,
            request.DisplayName,
            request.Password,
            request.Roles));
        return Results.Created($"/api/auth/users/{id}", new { userId = id });
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { message = ex.Message }); }
});

app.MapGet("/api/auth/google", (IOptions<OAuthOptions> options) =>
{
    if (string.IsNullOrWhiteSpace(options.Value.Google.ClientId))
        return Results.BadRequest(new { message = "Google OAuth is not configured." });

    return Results.Challenge(
        new AuthenticationProperties { RedirectUri = "/api/auth/google/callback" },
        [GoogleDefaults.AuthenticationScheme]);
});

app.MapGet("/api/auth/google/callback", HandleOAuthCallbackAsync(AuthProviders.Google));

app.MapGet("/api/auth/facebook", (IOptions<OAuthOptions> options) =>
{
    if (string.IsNullOrWhiteSpace(options.Value.Facebook.AppId))
        return Results.BadRequest(new { message = "Facebook OAuth is not configured." });

    return Results.Challenge(
        new AuthenticationProperties { RedirectUri = "/api/auth/facebook/callback" },
        [FacebookDefaults.AuthenticationScheme]);
});

app.MapGet("/api/auth/facebook/callback", HandleOAuthCallbackAsync(AuthProviders.Facebook));

app.MapAdminIdentityEndpoints();

app.Run();

static Func<HttpContext, IMediator, IOptions<OAuthOptions>, Task<IResult>> HandleOAuthCallbackAsync(string provider) =>
    async (context, mediator, oauthOptions) =>
    {
        var authResult = await context.AuthenticateAsync(ExternalScheme);
        if (!authResult.Succeeded || authResult.Principal is null)
            return Results.Redirect($"{oauthOptions.Value.FrontendCallbackUrl}?error=oauth_failed");

        var principal = authResult.Principal;
        var providerUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub")
            ?? string.Empty;
        var email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var displayName = principal.FindFirstValue(ClaimTypes.Name) ?? email;

        if (string.IsNullOrWhiteSpace(providerUserId) || string.IsNullOrWhiteSpace(email))
            return Results.Redirect($"{oauthOptions.Value.FrontendCallbackUrl}?error=missing_claims");

        try
        {
            var login = await mediator.Send(new HandleExternalLoginCommand(provider, providerUserId, email, displayName));
            var redirect = BuildFrontendRedirect(oauthOptions.Value.FrontendCallbackUrl, login);
            return Results.Redirect(redirect);
        }
        catch
        {
            return Results.Redirect($"{oauthOptions.Value.FrontendCallbackUrl}?error=login_failed");
        }
    };

static string BuildFrontendRedirect(string baseUrl, Auth.Application.Services.LoginTokenResult login)
{
    var separator = baseUrl.Contains('?') ? '&' : '?';
    return $"{baseUrl}{separator}token={Uri.EscapeDataString(login.Token)}&status=ok&authority={Uri.EscapeDataString(login.CurrentAuthority)}";
}

static object ToLoginResponse(Auth.Application.Services.LoginTokenResult result) => new
{
    status = "ok",
    token = result.Token,
    currentAuthority = result.CurrentAuthority,
    userId = result.UserId,
    email = result.Email,
    displayName = result.DisplayName,
    roles = result.Roles,
    permissions = result.Permissions,
    data = new
    {
        name = result.DisplayName,
        userid = result.UserId.ToString(),
        email = result.Email,
        access = result.CurrentAuthority,
        roles = result.Roles,
        permissions = result.Permissions
    }
};

internal sealed record LoginRequest(string Email, string Password);
internal sealed record RegisterRequest(string Email, string DisplayName, string Password, IReadOnlyList<string>? Roles = null);

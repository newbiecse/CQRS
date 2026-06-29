using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Auth.Application.Abstractions;

namespace Auth.Infrastructure.Http;

public sealed class UserServiceOptions
{
    public const string SectionName = "UserService";
    public string CommandsBaseUrl { get; set; } = "http://localhost:5206";
}

internal sealed record ProvisionUserApiRequest(Guid UserId, string Email, string DisplayName);

public sealed class HttpUserProfileProvisioner(
    HttpClient httpClient,
    IOptions<UserServiceOptions> options) : IUserProfileProvisioner
{
    public async Task ProvisionAsync(Guid userId, string email, string displayName, CancellationToken cancellationToken = default)
    {
        var baseUrl = options.Value.CommandsBaseUrl.TrimEnd('/');
        var response = await httpClient.PostAsJsonAsync(
            $"{baseUrl}/api/users/provision",
            new ProvisionUserApiRequest(userId, email, displayName),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Failed to provision user profile: {(int)response.StatusCode} {body}");
        }
    }

    public async Task UpdateDisplayNameAsync(Guid userId, string displayName, CancellationToken cancellationToken = default)
    {
        var baseUrl = options.Value.CommandsBaseUrl.TrimEnd('/');
        var response = await httpClient.PutAsJsonAsync(
            $"{baseUrl}/api/users/{userId}/profile",
            new { displayName },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Failed to update user profile: {(int)response.StatusCode} {body}");
        }
    }

    public async Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var baseUrl = options.Value.CommandsBaseUrl.TrimEnd('/');
        var response = await httpClient.PostAsync($"{baseUrl}/api/users/{userId}/deactivate", null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Failed to deactivate user profile: {(int)response.StatusCode} {body}");
        }
    }
}

public static class UserProfileProvisionerExtensions
{
    public static IServiceCollection AddUserProfileProvisioner(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<UserServiceOptions>(configuration.GetSection(UserServiceOptions.SectionName));
        services.AddHttpClient<IUserProfileProvisioner, HttpUserProfileProvisioner>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<UserServiceOptions>>().Value;
            client.BaseAddress = new Uri(options.CommandsBaseUrl.TrimEnd('/') + "/");
        });
        return services;
    }
}

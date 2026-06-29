using Auth.Application.Abstractions;
using Auth.Domain;
using CqrsDemo.BuildingBlocks.Auth;

namespace Auth.Application.Services;

public interface IAuthTokenIssuer
{
    Task<LoginTokenResult> IssueAsync(IdentityUser user, CancellationToken cancellationToken = default);
}

public sealed record LoginTokenResult(
    string Token,
    Guid UserId,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    string CurrentAuthority);

public sealed class AuthTokenIssuer(
    IJwtTokenService jwtTokenService,
    IAuthorizationStore authorizationStore) : IAuthTokenIssuer
{
    public async Task<LoginTokenResult> IssueAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        var roles = user.Roles;
        var permissions = await authorizationStore.ResolvePermissionsForRolesAsync(roles, cancellationToken);
        var authority = roles.FirstOrDefault() ?? PlatformRoles.Admin;
        var token = jwtTokenService.CreateToken(new AuthenticatedUser(
            user.Id,
            user.Email,
            user.DisplayName,
            roles,
            permissions));

        return new LoginTokenResult(
            token,
            user.Id,
            user.Email,
            user.DisplayName,
            roles,
            permissions,
            authority);
    }
}

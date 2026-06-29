using Auth.Domain;
using CqrsDemo.BuildingBlocks.Auth;

namespace Auth.Application.Services;

public interface IAuthTokenIssuer
{
    LoginTokenResult Issue(IdentityUser user);
}

public sealed record LoginTokenResult(
    string Token,
    Guid UserId,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles,
    string CurrentAuthority);

public sealed class AuthTokenIssuer(IJwtTokenService jwtTokenService) : IAuthTokenIssuer
{
    public LoginTokenResult Issue(IdentityUser user)
    {
        var roles = user.Roles;
        var authority = roles.FirstOrDefault() ?? PlatformRoles.Admin;
        var token = jwtTokenService.CreateToken(new AuthenticatedUser(
            user.Id,
            user.Email,
            user.DisplayName,
            roles));

        return new LoginTokenResult(token, user.Id, user.Email, user.DisplayName, roles, authority);
    }
}

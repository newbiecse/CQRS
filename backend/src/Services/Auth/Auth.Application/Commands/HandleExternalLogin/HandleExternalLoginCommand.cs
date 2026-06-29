using Auth.Application.Abstractions;
using Auth.Application.Services;
using Auth.Domain;
using MediatR;

namespace Auth.Application.Commands.HandleExternalLogin;

public sealed record HandleExternalLoginCommand(
    string Provider,
    string ProviderUserId,
    string Email,
    string DisplayName) : IRequest<LoginTokenResult>;

public sealed class HandleExternalLoginCommandHandler(
    IIdentityRepository repository,
    IUserProfileProvisioner profileProvisioner,
    IAuthTokenIssuer tokenIssuer)
    : IRequestHandler<HandleExternalLoginCommand, LoginTokenResult>
{
    public async Task<LoginTokenResult> Handle(HandleExternalLoginCommand request, CancellationToken cancellationToken)
    {
        var provider = request.Provider.Trim().ToLowerInvariant();
        var providerUserId = request.ProviderUserId.Trim();

        var existing = await repository.GetByExternalLoginAsync(provider, providerUserId, cancellationToken);
        if (existing is not null)
        {
            if (!existing.IsActive)
                throw new InvalidOperationException("Account is deactivated.");
            return tokenIssuer.Issue(existing);
        }

        var userByEmail = await repository.GetByEmailAsync(request.Email, cancellationToken);
        IdentityUser user;

        if (userByEmail is not null)
        {
            user = userByEmail;
        }
        else
        {
            user = IdentityUser.CreateFromExternal(Guid.NewGuid(), request.Email, request.DisplayName, ["user"]);
            await repository.SaveAsync(user, passwordHash: null, cancellationToken);
            await profileProvisioner.ProvisionAsync(user.Id, user.Email, user.DisplayName, cancellationToken);
        }

        await repository.SaveExternalLoginAsync(
            new ExternalLoginRecord(user.Id, provider, providerUserId, request.Email, DateTime.UtcNow),
            cancellationToken);

        return tokenIssuer.Issue(user);
    }
}

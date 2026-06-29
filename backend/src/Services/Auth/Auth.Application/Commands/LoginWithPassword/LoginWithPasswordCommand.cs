using Auth.Application.Abstractions;
using Auth.Application.Services;
using MediatR;

namespace Auth.Application.Commands.LoginWithPassword;

public sealed record LoginWithPasswordCommand(string Email, string Password) : IRequest<LoginTokenResult?>;

public sealed class LoginWithPasswordCommandHandler(
    IIdentityRepository repository,
    IPasswordHasher passwordHasher,
    IAuthTokenIssuer tokenIssuer)
    : IRequestHandler<LoginWithPasswordCommand, LoginTokenResult?>
{
    public async Task<LoginTokenResult?> Handle(LoginWithPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return null;

        var user = await repository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !user.IsActive)
            return null;

        var hash = await repository.GetPasswordHashAsync(user.Id, cancellationToken);
        if (hash is null || !passwordHasher.Verify(hash, request.Password))
            return null;

        return tokenIssuer.Issue(user);
    }
}

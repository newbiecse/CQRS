using Auth.Application.Abstractions;
using Auth.Domain;
using CqrsDemo.BuildingBlocks.Auth;
using MediatR;

namespace Auth.Application.Commands.RegisterLocalUser;

public sealed record RegisterLocalUserCommand(
    string Email,
    string DisplayName,
    string Password,
    IReadOnlyList<string>? Roles = null,
    Guid? UserId = null) : IRequest<Guid>;

public sealed class RegisterLocalUserCommandHandler(
    IIdentityRepository repository,
    IPasswordHasher passwordHasher,
    IUserProfileProvisioner profileProvisioner)
    : IRequestHandler<RegisterLocalUserCommand, Guid>
{
    public async Task<Guid> Handle(RegisterLocalUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await repository.ExistsByEmailAsync(email, cancellationToken))
            throw new InvalidOperationException($"Email '{email}' is already registered.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters.", nameof(request.Password));

        var userId = request.UserId ?? Guid.NewGuid();
        var user = IdentityUser.CreateLocal(userId, email, request.DisplayName, request.Roles);
        var hash = passwordHasher.Hash(request.Password);

        await repository.SaveAsync(user, hash, cancellationToken);
        await profileProvisioner.ProvisionAsync(user.Id, user.Email, user.DisplayName, cancellationToken);

        return user.Id;
    }
}

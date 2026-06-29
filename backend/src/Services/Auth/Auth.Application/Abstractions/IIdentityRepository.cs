using Auth.Domain;

namespace Auth.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string passwordHash, string password);
}

public interface IIdentityRepository
{
    Task<IdentityUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IdentityUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<string?> GetPasswordHashAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SaveAsync(IdentityUser user, string? passwordHash, CancellationToken cancellationToken = default);
    Task SaveExternalLoginAsync(ExternalLoginRecord login, CancellationToken cancellationToken = default);
    Task<ExternalLoginRecord?> GetExternalLoginAsync(string provider, string providerUserId, CancellationToken cancellationToken = default);
    Task<IdentityUser?> GetByExternalLoginAsync(string provider, string providerUserId, CancellationToken cancellationToken = default);
}

public sealed record ExternalLoginRecord(
    Guid UserId,
    string Provider,
    string ProviderUserId,
    string? Email,
    DateTime LinkedAt);

public interface IUserProfileProvisioner
{
    Task ProvisionAsync(Guid userId, string email, string displayName, CancellationToken cancellationToken = default);
}

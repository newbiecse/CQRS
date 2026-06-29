using Auth.Application.Abstractions;
using Auth.Domain;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public sealed class SqlIdentityRepository(IdentityDbContext db) : IIdentityRepository
{
    public async Task<IdentityUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<IdentityUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var entity = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return db.Users.AnyAsync(u => u.Email == normalized, cancellationToken);
    }

    public async Task<string?> GetPasswordHashAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cred = await db.LocalCredentials.AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        return cred?.PasswordHash;
    }

    public async Task SaveAsync(IdentityUser user, string? passwordHash, CancellationToken cancellationToken = default)
    {
        var entity = await db.Users.FindAsync([user.Id], cancellationToken);
        if (entity is null)
        {
            db.Users.Add(ToEntity(user));
            if (!string.IsNullOrWhiteSpace(passwordHash))
            {
                db.LocalCredentials.Add(new LocalCredentialEntity
                {
                    UserId = user.Id,
                    PasswordHash = passwordHash
                });
            }
        }
        else
        {
            entity.Email = user.Email;
            entity.DisplayName = user.DisplayName;
            entity.RolesCsv = user.RolesCsv;
            entity.IsActive = user.IsActive;
            entity.CreatedAt = user.CreatedAt;

            if (!string.IsNullOrWhiteSpace(passwordHash))
            {
                var cred = await db.LocalCredentials.FindAsync([user.Id], cancellationToken);
                if (cred is null)
                    db.LocalCredentials.Add(new LocalCredentialEntity { UserId = user.Id, PasswordHash = passwordHash });
                else
                    cred.PasswordHash = passwordHash;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveExternalLoginAsync(ExternalLoginRecord login, CancellationToken cancellationToken = default)
    {
        var exists = await db.ExternalLogins.AnyAsync(
            e => e.Provider == login.Provider && e.ProviderUserId == login.ProviderUserId,
            cancellationToken);
        if (exists) return;

        db.ExternalLogins.Add(new ExternalLoginEntity
        {
            UserId = login.UserId,
            Provider = login.Provider,
            ProviderUserId = login.ProviderUserId,
            Email = login.Email,
            LinkedAt = login.LinkedAt
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ExternalLoginRecord?> GetExternalLoginAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.ExternalLogins.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Provider == provider && e.ProviderUserId == providerUserId, cancellationToken);
        return entity is null
            ? null
            : new ExternalLoginRecord(entity.UserId, entity.Provider, entity.ProviderUserId, entity.Email, entity.LinkedAt);
    }

    public async Task<IdentityUser?> GetByExternalLoginAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default)
    {
        var link = await db.ExternalLogins.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Provider == provider && e.ProviderUserId == providerUserId, cancellationToken);
        if (link is null) return null;

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == link.UserId, cancellationToken);
        return user is null ? null : ToDomain(user);
    }

    private static IdentityUser ToDomain(IdentityUserEntity entity) =>
        IdentityUser.Restore(entity.Id, entity.Email, entity.DisplayName, entity.RolesCsv, entity.IsActive, entity.CreatedAt);

    private static IdentityUserEntity ToEntity(IdentityUser user) =>
        new()
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            RolesCsv = user.RolesCsv,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
}

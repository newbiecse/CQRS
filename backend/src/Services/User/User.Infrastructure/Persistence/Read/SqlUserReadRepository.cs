using Microsoft.EntityFrameworkCore;
using User.Application.Abstractions;
using User.Application.ReadModels;

namespace User.Infrastructure.Persistence.Read;

public sealed class SqlUserReadRepository(UserReadDbContext db) : IUserReadRepository
{
    public async Task UpsertAsync(UserReadModel user, CancellationToken ct = default)
    {
        var entity = await db.Users.FindAsync([user.Id], ct);
        if (entity is null)
        {
            entity = new UserReadEntity { Id = user.Id };
            db.Users.Add(entity);
        }

        entity.Email = user.Email;
        entity.DisplayName = user.DisplayName;
        entity.IsActive = user.IsActive;
        entity.RegisteredAt = user.RegisteredAt;
        entity.LastUpdatedAt = user.LastUpdatedAt;
        await db.SaveChangesAsync(ct);
    }

    public async Task<UserReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<UserReadModel?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var entity = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == normalized, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<IReadOnlyList<UserReadModel>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return [];

        var list = await db.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<UserReadModel>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await db.Users.AsNoTracking().OrderBy(u => u.Email).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    private static UserReadModel Map(UserReadEntity entity) => new()
    {
        Id = entity.Id,
        Email = entity.Email,
        DisplayName = entity.DisplayName,
        IsActive = entity.IsActive,
        RegisteredAt = entity.RegisteredAt,
        LastUpdatedAt = entity.LastUpdatedAt
    };
}

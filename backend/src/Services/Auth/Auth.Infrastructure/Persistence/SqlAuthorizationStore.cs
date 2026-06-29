using Auth.Application.Abstractions;
using Auth.Domain;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

public sealed class SqlAuthorizationStore(IdentityDbContext db) : IAuthorizationStore
{
    public async Task<IReadOnlyList<RoleAdminDto>> ListRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync(cancellationToken);
        var rolePermissions = await LoadRolePermissionMapAsync(cancellationToken);
        return roles.Select(r => ToRoleDto(r, rolePermissions.GetValueOrDefault(r.Id, []))).ToList();
    }

    public async Task<RoleAdminDto?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (role is null) return null;

        var permissions = await GetPermissionNamesForRoleAsync(role.Id, cancellationToken);
        return ToRoleDto(role, permissions);
    }

    public async Task<RoleAdminDto?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();
        var role = await db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Name == normalized, cancellationToken);
        if (role is null) return null;

        var permissions = await GetPermissionNamesForRoleAsync(role.Id, cancellationToken);
        return ToRoleDto(role, permissions);
    }

    public async Task SaveRoleAsync(RoleDefinition role, CancellationToken cancellationToken = default)
    {
        var entity = await db.Roles.FindAsync([role.Id], cancellationToken);
        if (entity is null)
            db.Roles.Add(ToRoleEntity(role));
        else
        {
            entity.Description = role.Description;
            entity.IsSystem = role.IsSystem;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await db.Roles.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Role '{id}' was not found.");

        if (role.IsSystem)
            throw new InvalidOperationException("System roles cannot be deleted.");

        db.Roles.Remove(role);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SetRolePermissionsAsync(
        Guid roleId,
        IReadOnlyList<Guid> permissionIds,
        CancellationToken cancellationToken = default)
    {
        var existing = await db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(cancellationToken);
        db.RolePermissions.RemoveRange(existing);

        foreach (var permissionId in permissionIds.Distinct())
        {
            db.RolePermissions.Add(new RolePermissionEntity { RoleId = roleId, PermissionId = permissionId });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PermissionAdminDto>> ListPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await db.Permissions.AsNoTracking().OrderBy(p => p.Name).ToListAsync(cancellationToken);
        return permissions.Select(ToPermissionDto).ToList();
    }

    public async Task<PermissionAdminDto?> GetPermissionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permission = await db.Permissions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return permission is null ? null : ToPermissionDto(permission);
    }

    public async Task<PermissionAdminDto?> GetPermissionByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();
        var permission = await db.Permissions.AsNoTracking().FirstOrDefaultAsync(p => p.Name == normalized, cancellationToken);
        return permission is null ? null : ToPermissionDto(permission);
    }

    public async Task SavePermissionAsync(PermissionDefinition permission, CancellationToken cancellationToken = default)
    {
        var entity = await db.Permissions.FindAsync([permission.Id], cancellationToken);
        if (entity is null)
            db.Permissions.Add(ToPermissionEntity(permission));
        else
            entity.Description = permission.Description;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePermissionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permission = await db.Permissions.FindAsync([id], cancellationToken)
            ?? throw new KeyNotFoundException($"Permission '{id}' was not found.");

        db.Permissions.Remove(permission);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> ResolvePermissionsForRolesAsync(
        IReadOnlyList<string> roleNames,
        CancellationToken cancellationToken = default)
    {
        if (roleNames.Count == 0) return [];

        var normalized = roleNames.Select(r => r.Trim().ToLowerInvariant()).Distinct().ToList();
        var roleIds = await db.Roles.AsNoTracking()
            .Where(r => normalized.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0) return [];

        return await db.RolePermissions.AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Join(db.Permissions.AsNoTracking(), rp => rp.PermissionId, p => p.Id, (_, p) => p.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<Guid, List<string>>> LoadRolePermissionMapAsync(CancellationToken cancellationToken)
    {
        var rows = await db.RolePermissions.AsNoTracking()
            .Join(db.Permissions.AsNoTracking(), rp => rp.PermissionId, p => p.Id, (rp, p) => new { rp.RoleId, p.Name })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.RoleId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Name).OrderBy(n => n).ToList());
    }

    private async Task<List<string>> GetPermissionNamesForRoleAsync(Guid roleId, CancellationToken cancellationToken) =>
        await db.RolePermissions.AsNoTracking()
            .Where(rp => rp.RoleId == roleId)
            .Join(db.Permissions.AsNoTracking(), rp => rp.PermissionId, p => p.Id, (_, p) => p.Name)
            .OrderBy(n => n)
            .ToListAsync(cancellationToken);

    private static RoleAdminDto ToRoleDto(RoleEntity entity, IReadOnlyList<string> permissions) =>
        new(entity.Id, entity.Name, entity.Description, entity.IsSystem, entity.CreatedAt, permissions);

    private static PermissionAdminDto ToPermissionDto(PermissionEntity entity) =>
        new(entity.Id, entity.Name, entity.Description, entity.CreatedAt);

    private static RoleEntity ToRoleEntity(RoleDefinition role) =>
        new()
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            CreatedAt = role.CreatedAt
        };

    private static PermissionEntity ToPermissionEntity(PermissionDefinition permission) =>
        new()
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            CreatedAt = permission.CreatedAt
        };
}

using Auth.Application.Abstractions;
using Auth.Domain;

namespace Auth.Application.Admin;

public sealed class AuthorizationAdminService(
    IIdentityRepository identityRepository,
    IAuthorizationStore authorizationStore,
    IPasswordHasher passwordHasher,
    IUserProfileProvisioner profileProvisioner) : IAuthorizationAdminService
{
    public async Task<IReadOnlyList<UserAdminDto>> ListUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await identityRepository.ListAllAsync(cancellationToken);
        var results = new List<UserAdminDto>(users.Count);

        foreach (var user in users)
        {
            var permissions = await authorizationStore.ResolvePermissionsForRolesAsync(user.Roles, cancellationToken);
            results.Add(ToUserDto(user, permissions));
        }

        return results;
    }

    public async Task<UserAdminDto?> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await identityRepository.GetByIdAsync(id, cancellationToken);
        if (user is null) return null;

        var permissions = await authorizationStore.ResolvePermissionsForRolesAsync(user.Roles, cancellationToken);
        return ToUserDto(user, permissions);
    }

    public async Task UpdateUserAsync(UpdateUserAdminRequest request, CancellationToken cancellationToken = default)
    {
        var user = await identityRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{request.UserId}' was not found.");

        await ValidateRolesAsync(request.Roles, cancellationToken);

        var displayNameChanged = !string.Equals(user.DisplayName, request.DisplayName, StringComparison.Ordinal);
        var wasActive = user.IsActive;

        user.UpdateDisplayName(request.DisplayName);
        user.UpdateRoles(request.Roles);

        if (request.IsActive)
            user.Activate();
        else
            user.Deactivate();

        string? passwordHash = null;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (request.Password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters.", nameof(request.Password));
            passwordHash = passwordHasher.Hash(request.Password);
        }

        await identityRepository.SaveAsync(user, passwordHash, cancellationToken);

        if (displayNameChanged)
            await profileProvisioner.UpdateDisplayNameAsync(user.Id, user.DisplayName, cancellationToken);

        if (wasActive && !request.IsActive)
            await profileProvisioner.DeactivateAsync(user.Id, cancellationToken);
    }

    public async Task DeactivateUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await identityRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{id}' was not found.");

        if (!user.IsActive)
            return;

        user.Deactivate();
        await identityRepository.SaveAsync(user, null, cancellationToken);
        await profileProvisioner.DeactivateAsync(id, cancellationToken);
    }

    public Task<IReadOnlyList<RoleAdminDto>> ListRolesAsync(CancellationToken cancellationToken = default) =>
        authorizationStore.ListRolesAsync(cancellationToken);

    public Task<RoleAdminDto?> GetRoleAsync(Guid id, CancellationToken cancellationToken = default) =>
        authorizationStore.GetRoleByIdAsync(id, cancellationToken);

    public async Task<Guid> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await authorizationStore.GetRoleByNameAsync(request.Name, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Role '{request.Name}' already exists.");

        var role = RoleDefinition.Create(request.Name, request.Description);
        await authorizationStore.SaveRoleAsync(role, cancellationToken);
        return role.Id;
    }

    public async Task UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await authorizationStore.GetRoleByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Role '{id}' was not found.");

        var role = RoleDefinition.Restore(existing.Id, existing.Name, request.Description, existing.IsSystem, existing.CreatedAt);
        await authorizationStore.SaveRoleAsync(role, cancellationToken);
    }

    public async Task DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await authorizationStore.GetRoleByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Role '{id}' was not found.");

        await authorizationStore.DeleteRoleAsync(id, cancellationToken);
        await RemoveRoleFromUsersAsync(role.Name, cancellationToken);
    }

    public async Task SetRolePermissionsAsync(
        Guid roleId,
        IReadOnlyList<string> permissionNames,
        CancellationToken cancellationToken = default)
    {
        _ = await authorizationStore.GetRoleByIdAsync(roleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Role '{roleId}' was not found.");

        var permissionIds = new List<Guid>();
        foreach (var name in permissionNames.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var permission = await authorizationStore.GetPermissionByNameAsync(name, cancellationToken)
                ?? throw new KeyNotFoundException($"Permission '{name}' was not found.");
            permissionIds.Add(permission.Id);
        }

        await authorizationStore.SetRolePermissionsAsync(roleId, permissionIds, cancellationToken);
    }

    public Task<IReadOnlyList<PermissionAdminDto>> ListPermissionsAsync(CancellationToken cancellationToken = default) =>
        authorizationStore.ListPermissionsAsync(cancellationToken);

    public Task<PermissionAdminDto?> GetPermissionAsync(Guid id, CancellationToken cancellationToken = default) =>
        authorizationStore.GetPermissionByIdAsync(id, cancellationToken);

    public async Task<Guid> CreatePermissionAsync(CreatePermissionRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await authorizationStore.GetPermissionByNameAsync(request.Name, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Permission '{request.Name}' already exists.");

        var permission = PermissionDefinition.Create(request.Name, request.Description);
        await authorizationStore.SavePermissionAsync(permission, cancellationToken);
        return permission.Id;
    }

    public async Task UpdatePermissionAsync(Guid id, UpdatePermissionRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await authorizationStore.GetPermissionByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Permission '{id}' was not found.");

        var permission = PermissionDefinition.Restore(existing.Id, existing.Name, request.Description, existing.CreatedAt);
        await authorizationStore.SavePermissionAsync(permission, cancellationToken);
    }

    public Task DeletePermissionAsync(Guid id, CancellationToken cancellationToken = default) =>
        authorizationStore.DeletePermissionAsync(id, cancellationToken);

    public Task<IReadOnlyList<string>> ResolvePermissionsForRolesAsync(
        IReadOnlyList<string> roleNames,
        CancellationToken cancellationToken = default) =>
        authorizationStore.ResolvePermissionsForRolesAsync(roleNames, cancellationToken);

    private async Task ValidateRolesAsync(IReadOnlyList<string> roles, CancellationToken cancellationToken)
    {
        foreach (var roleName in roles)
        {
            var role = await authorizationStore.GetRoleByNameAsync(roleName, cancellationToken);
            if (role is null)
                throw new ArgumentException($"Unknown role '{roleName}'.", nameof(roles));
        }
    }

    private async Task RemoveRoleFromUsersAsync(string roleName, CancellationToken cancellationToken)
    {
        var users = await identityRepository.ListAllAsync(cancellationToken);
        foreach (var user in users.Where(u => u.Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase)))
        {
            var updatedRoles = user.Roles.Where(r => !string.Equals(r, roleName, StringComparison.OrdinalIgnoreCase)).ToArray();
            user.UpdateRoles(updatedRoles);
            await identityRepository.SaveAsync(user, null, cancellationToken);
        }
    }

    private static UserAdminDto ToUserDto(IdentityUser user, IReadOnlyList<string> permissions) =>
        new(user.Id, user.Email, user.DisplayName, user.IsActive, user.CreatedAt, user.Roles, permissions);
}

using Auth.Domain;

namespace Auth.Application.Abstractions;

public interface IAuthorizationAdminService
{
    Task<IReadOnlyList<UserAdminDto>> ListUsersAsync(CancellationToken cancellationToken = default);
    Task<UserAdminDto?> GetUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(UpdateUserAdminRequest request, CancellationToken cancellationToken = default);
    Task DeactivateUserAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoleAdminDto>> ListRolesAsync(CancellationToken cancellationToken = default);
    Task<RoleAdminDto?> GetRoleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default);
    Task SetRolePermissionsAsync(Guid roleId, IReadOnlyList<string> permissionNames, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PermissionAdminDto>> ListPermissionsAsync(CancellationToken cancellationToken = default);
    Task<PermissionAdminDto?> GetPermissionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreatePermissionAsync(CreatePermissionRequest request, CancellationToken cancellationToken = default);
    Task UpdatePermissionAsync(Guid id, UpdatePermissionRequest request, CancellationToken cancellationToken = default);
    Task DeletePermissionAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ResolvePermissionsForRolesAsync(
        IReadOnlyList<string> roleNames,
        CancellationToken cancellationToken = default);
}

public sealed record UserAdminDto(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsActive,
    DateTime CreatedAt,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public sealed record RoleAdminDto(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    DateTime CreatedAt,
    IReadOnlyList<string> Permissions);

public sealed record PermissionAdminDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt);

public sealed record UpdateUserAdminRequest(
    Guid UserId,
    string DisplayName,
    IReadOnlyList<string> Roles,
    bool IsActive,
    string? Password = null);

public sealed record CreateRoleRequest(string Name, string Description);
public sealed record UpdateRoleRequest(string Description);
public sealed record CreatePermissionRequest(string Name, string Description);
public sealed record UpdatePermissionRequest(string Description);

public interface IAuthorizationStore
{
    Task<IReadOnlyList<RoleAdminDto>> ListRolesAsync(CancellationToken cancellationToken = default);
    Task<RoleAdminDto?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleAdminDto?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default);
    Task SaveRoleAsync(RoleDefinition role, CancellationToken cancellationToken = default);
    Task DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default);
    Task SetRolePermissionsAsync(Guid roleId, IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PermissionAdminDto>> ListPermissionsAsync(CancellationToken cancellationToken = default);
    Task<PermissionAdminDto?> GetPermissionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PermissionAdminDto?> GetPermissionByNameAsync(string name, CancellationToken cancellationToken = default);
    Task SavePermissionAsync(PermissionDefinition permission, CancellationToken cancellationToken = default);
    Task DeletePermissionAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ResolvePermissionsForRolesAsync(
        IReadOnlyList<string> roleNames,
        CancellationToken cancellationToken = default);
}

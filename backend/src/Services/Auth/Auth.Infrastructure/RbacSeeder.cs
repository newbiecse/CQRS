using Auth.Application.Abstractions;
using Auth.Domain;
using CqrsDemo.BuildingBlocks.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public static class RbacSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IAuthorizationStore>();

        await SeedPermissionsAsync(store, cancellationToken);
        await SeedRolesAsync(store, cancellationToken);
    }

    private static async Task SeedPermissionsAsync(IAuthorizationStore store, CancellationToken cancellationToken)
    {
        var defaults = new (string Name, string Description)[]
        {
            (PlatformPermissions.ManageCatalog, "Manage product catalog and inventory"),
            (PlatformPermissions.ManageOrders, "Manage customer orders"),
            (PlatformPermissions.ManageUsers, "Manage platform users"),
            (PlatformPermissions.ManageRoles, "Manage roles and role assignments"),
            (PlatformPermissions.ManagePermissions, "Manage permission claims")
        };

        foreach (var (name, description) in defaults)
        {
            if (await store.GetPermissionByNameAsync(name, cancellationToken) is not null)
                continue;

            await store.SavePermissionAsync(PermissionDefinition.Create(name, description), cancellationToken);
        }
    }

    private static async Task SeedRolesAsync(IAuthorizationStore store, CancellationToken cancellationToken)
    {
        await EnsureRoleAsync(
            store,
            PlatformRoles.Admin,
            "Full platform administrator",
            isSystem: true,
            [
                PlatformPermissions.ManageCatalog,
                PlatformPermissions.ManageOrders,
                PlatformPermissions.ManageUsers,
                PlatformPermissions.ManageRoles,
                PlatformPermissions.ManagePermissions
            ],
            cancellationToken);

        await EnsureRoleAsync(
            store,
            PlatformRoles.CatalogManager,
            "Catalog and inventory manager",
            isSystem: true,
            [PlatformPermissions.ManageCatalog],
            cancellationToken);

        await EnsureRoleAsync(
            store,
            PlatformRoles.OrderManager,
            "Order operations manager",
            isSystem: true,
            [PlatformPermissions.ManageOrders],
            cancellationToken);

        await EnsureRoleAsync(
            store,
            "user",
            "Default authenticated user",
            isSystem: true,
            [],
            cancellationToken);
    }

    private static async Task EnsureRoleAsync(
        IAuthorizationStore store,
        string name,
        string description,
        bool isSystem,
        IReadOnlyList<string> permissions,
        CancellationToken cancellationToken)
    {
        var existing = await store.GetRoleByNameAsync(name, cancellationToken);
        Guid roleId;

        if (existing is null)
        {
            var role = RoleDefinition.Create(name, description, isSystem);
            await store.SaveRoleAsync(role, cancellationToken);
            roleId = role.Id;
        }
        else
        {
            roleId = existing.Id;
        }

        var permissionIds = new List<Guid>();
        foreach (var permissionName in permissions)
        {
            var permission = await store.GetPermissionByNameAsync(permissionName, cancellationToken);
            if (permission is not null)
                permissionIds.Add(permission.Id);
        }

        await store.SetRolePermissionsAsync(roleId, permissionIds, cancellationToken);
    }
}

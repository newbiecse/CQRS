namespace CqrsDemo.BuildingBlocks.Auth;

public static class PlatformPermissions
{
    public const string ManageCatalog = "catalog.manage";
    public const string ManageOrders = "orders.manage";
    public const string ManageUsers = "users.manage";
    public const string ManageRoles = "roles.manage";
    public const string ManagePermissions = "permissions.manage";
}

public static class PlatformClaimTypes
{
    public const string Permission = "permission";
}

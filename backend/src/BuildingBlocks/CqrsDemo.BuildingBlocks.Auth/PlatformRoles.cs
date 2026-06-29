namespace CqrsDemo.BuildingBlocks.Auth;

public static class PlatformRoles
{
    public const string Admin = "admin";
    public const string CatalogManager = "catalog-manager";
    public const string OrderManager = "order-manager";
}

public static class PlatformPolicies
{
    public const string Authenticated = "Authenticated";
    public const string AdminOnly = "AdminOnly";
    public const string CanManageCatalog = "CanManageCatalog";
    public const string CanManageOrders = "CanManageOrders";
}

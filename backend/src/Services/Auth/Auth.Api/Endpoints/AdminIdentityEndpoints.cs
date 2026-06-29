using Auth.Application.Abstractions;
using CqrsDemo.BuildingBlocks.Auth;

namespace Auth.Api.Endpoints;

internal static class AdminIdentityEndpoints
{
    public static void MapAdminIdentityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth/admin").RequireAuthorization(PlatformPolicies.AdminOnly);

        group.MapGet("/users", async (IAuthorizationAdminService service, CancellationToken ct) =>
            Results.Ok(await service.ListUsersAsync(ct)));

        group.MapGet("/users/{id:guid}", async (Guid id, IAuthorizationAdminService service, CancellationToken ct) =>
        {
            var user = await service.GetUserAsync(id, ct);
            return user is null ? Results.NotFound() : Results.Ok(user);
        });

        group.MapPut("/users/{id:guid}", async (
            Guid id,
            UpdateUserRequest request,
            IAuthorizationAdminService service,
            CancellationToken ct) =>
        {
            try
            {
                await service.UpdateUserAsync(
                    new UpdateUserAdminRequest(id, request.DisplayName, request.Roles ?? [], request.IsActive, request.Password),
                    ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
        });

        group.MapPost("/users/{id:guid}/deactivate", async (
            Guid id,
            IAuthorizationAdminService service,
            CancellationToken ct) =>
        {
            try
            {
                await service.DeactivateUserAsync(id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
        });

        group.MapGet("/roles", async (IAuthorizationAdminService service, CancellationToken ct) =>
            Results.Ok(await service.ListRolesAsync(ct)));

        group.MapGet("/roles/{id:guid}", async (Guid id, IAuthorizationAdminService service, CancellationToken ct) =>
        {
            var role = await service.GetRoleAsync(id, ct);
            return role is null ? Results.NotFound() : Results.Ok(role);
        });

        group.MapPost("/roles", async (CreateRoleRequest request, IAuthorizationAdminService service, CancellationToken ct) =>
        {
            try
            {
                var id = await service.CreateRoleAsync(request, ct);
                return Results.Created($"/api/auth/admin/roles/{id}", new { roleId = id });
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { message = ex.Message }); }
        });

        group.MapPut("/roles/{id:guid}", async (
            Guid id,
            UpdateRoleRequest request,
            IAuthorizationAdminService service,
            CancellationToken ct) =>
        {
            try
            {
                await service.UpdateRoleAsync(id, request, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
        });

        group.MapDelete("/roles/{id:guid}", async (Guid id, IAuthorizationAdminService service, CancellationToken ct) =>
        {
            try
            {
                await service.DeleteRoleAsync(id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { message = ex.Message }); }
        });

        group.MapPut("/roles/{id:guid}/permissions", async (
            Guid id,
            SetRolePermissionsRequest request,
            IAuthorizationAdminService service,
            CancellationToken ct) =>
        {
            try
            {
                await service.SetRolePermissionsAsync(id, request.Permissions ?? [], ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
        });

        group.MapGet("/permissions", async (IAuthorizationAdminService service, CancellationToken ct) =>
            Results.Ok(await service.ListPermissionsAsync(ct)));

        group.MapGet("/permissions/{id:guid}", async (Guid id, IAuthorizationAdminService service, CancellationToken ct) =>
        {
            var permission = await service.GetPermissionAsync(id, ct);
            return permission is null ? Results.NotFound() : Results.Ok(permission);
        });

        group.MapPost("/permissions", async (CreatePermissionRequest request, IAuthorizationAdminService service, CancellationToken ct) =>
        {
            try
            {
                var id = await service.CreatePermissionAsync(request, ct);
                return Results.Created($"/api/auth/admin/permissions/{id}", new { permissionId = id });
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { message = ex.Message }); }
        });

        group.MapPut("/permissions/{id:guid}", async (
            Guid id,
            UpdatePermissionRequest request,
            IAuthorizationAdminService service,
            CancellationToken ct) =>
        {
            try
            {
                await service.UpdatePermissionAsync(id, request, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
        });

        group.MapDelete("/permissions/{id:guid}", async (Guid id, IAuthorizationAdminService service, CancellationToken ct) =>
        {
            try
            {
                await service.DeletePermissionAsync(id, ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
        });
    }

    internal sealed record UpdateUserRequest(
        string DisplayName,
        IReadOnlyList<string>? Roles,
        bool IsActive,
        string? Password = null);

    internal sealed record SetRolePermissionsRequest(IReadOnlyList<string>? Permissions);
}

using Audit.Application.Abstractions;
using Microsoft.Extensions.Options;
using Shop.Admin.Api.Clients;
using Shop.Admin.Api.Options;
using System.Text.Json;

namespace Shop.Admin.Api.Endpoints;

internal static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin");

        group.MapGet("/dashboard", GetDashboardAsync);

        group.MapGet("/products", (AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.GetAsync(options.Value.ProductQueries, "/api/products", ct));

        group.MapGet("/products/{id:guid}", (Guid id, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.GetAsync(options.Value.ProductQueries, $"/api/products/{id}", ct));

        group.MapPost("/products", (CreateProductRequest request, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.PostAsync(options.Value.ProductCommands, "/api/products", request, ct));

        group.MapPut("/products/{id:guid}/price", (Guid id, UpdatePriceRequest request, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.PutAsync(options.Value.ProductCommands, $"/api/products/{id}/price", request, ct));

        group.MapPut("/products/{id:guid}", (Guid id, UpdateProductRequest request, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.PutAsync(options.Value.ProductCommands, $"/api/products/{id}", request, ct));

        group.MapDelete("/products/{id:guid}", (Guid id, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.DeleteAsync(options.Value.ProductCommands, $"/api/products/{id}", ct));

        group.MapGet("/users", (AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.GetAsync(options.Value.UserQueries, "/api/users", ct));

        group.MapPost("/users", (RegisterUserRequest request, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.PostAsync(options.Value.UserCommands, "/api/users", request, ct));

        group.MapPost("/users/{userId:guid}/deactivate", (Guid userId, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.PostAsync(options.Value.UserCommands, $"/api/users/{userId}/deactivate", new { }, ct));

        group.MapGet("/orders", (AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.GetAsync(options.Value.OrderQueries, "/api/orders", ct));

        group.MapGet("/orders/{id:guid}", (Guid id, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.GetAsync(options.Value.OrderQueries, $"/api/orders/{id}", ct));

        group.MapPost("/orders", (CreateOrderRequest request, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.PostAsync(options.Value.OrderCommands, "/api/orders", request, ct));

        group.MapPut("/orders/{orderId:guid}", (Guid orderId, UpdateOrderRequest request, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.PutAsync(options.Value.OrderCommands, $"/api/orders/{orderId}", request, ct));

        group.MapDelete("/orders/{orderId:guid}", (Guid orderId, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.DeleteAsync(options.Value.OrderCommands, $"/api/orders/{orderId}", ct));

        group.MapPost("/orders/{orderId:guid}/cancel", (Guid orderId, CancelOrderRequest request, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.PostAsync(options.Value.OrderCommands, $"/api/orders/{orderId}/cancel", request, ct));

        group.MapPost("/orders/{orderId:guid}/mark-paid", (Guid orderId, MarkOrderPaidRequest request, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.PostAsync(options.Value.OrderCommands, $"/api/orders/{orderId}/mark-paid", request, ct));

        group.MapGet("/inventory", (AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.GetAsync(options.Value.InventoryQueries, "/api/inventory", ct));

        group.MapGet("/inventory/{productId:guid}", (Guid productId, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.GetAsync(options.Value.InventoryQueries, $"/api/inventory/{productId}", ct));

        group.MapPut("/inventory/{productId:guid}", (Guid productId, AdjustInventoryRequest request, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.PutAsync(options.Value.InventoryCommands, $"/api/inventory/{productId}", request, ct));

        group.MapGet("/carts/{id:guid}", (Guid id, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.GetAsync(options.Value.CartQueries, $"/api/carts/{id}", ct));

        group.MapPost("/carts", (CreateCartRequest request, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.PostAsync(options.Value.CartCommands, "/api/carts", request, ct));

        group.MapPost("/carts/{cartId:guid}/items", (Guid cartId, AddCartItemRequest request, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.PostAsync(options.Value.CartCommands, $"/api/carts/{cartId}/items", request, ct));

        group.MapPost("/carts/{cartId:guid}/checkout", (Guid cartId, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.PostAsync(options.Value.CartCommands, $"/api/carts/{cartId}/checkout", new { }, ct));

        group.MapGet("/reports/top-users/{period}", (string period, int? limit, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
        {
            var query = limit.HasValue ? $"?limit={limit.Value}" : string.Empty;
            return client.GetAsync(options.Value.ReportingQueries, $"/api/reports/top-users/{period}{query}", ct);
        });

        group.MapGet("/sagas/{sagaId:guid}", (Guid sagaId, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.GetAsync(options.Value.CheckoutSaga, $"/api/sagas/{sagaId}", ct));

        group.MapPost("/sagas/checkout", (StartCheckoutSagaRequest request, AdminBackendClient client, IOptions<AdminShopServiceOptions> options, CancellationToken ct) =>
            client.PostAsync(options.Value.CheckoutSaga, "/api/sagas/checkout", request, ct));

        group.MapGet("/audit", SearchAuditAsync);
    }

    private static async Task<IResult> SearchAuditAsync(
        string? entityType,
        string? entityId,
        string? eventType,
        string? q,
        int? size,
        IBusinessAuditReader reader,
        CancellationToken cancellationToken)
    {
        try
        {
            var results = await reader.SearchAsync(
                new BusinessAuditSearchQuery
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    EventType = eventType,
                    SearchText = q,
                    Size = size ?? 50
                },
                cancellationToken);

            return Results.Ok(results);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                title: "Elasticsearch audit search failed",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private static async Task<IResult> GetDashboardAsync(
        AdminBackendClient client,
        IOptions<AdminShopServiceOptions> options,
        CancellationToken cancellationToken)
    {
        var services = options.Value;

        try
        {
            var productsTask = client.ReadJsonAsync<JsonElement[]>(services.ProductQueries, "/api/products", cancellationToken);
            var ordersTask = client.ReadJsonAsync<JsonElement[]>(services.OrderQueries, "/api/orders", cancellationToken);
            var topUsersTask = client.ReadJsonAsync<JsonElement[]>(
                services.ReportingQueries,
                "/api/reports/top-users/week?limit=5",
                cancellationToken);

            await Task.WhenAll(productsTask, ordersTask, topUsersTask);

            return Results.Ok(new
            {
                productCount = productsTask.Result?.Length ?? 0,
                orderCount = ordersTask.Result?.Length ?? 0,
                topUsers = topUsersTask.Result ?? Array.Empty<JsonElement>()
            });
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                title: "Failed to load dashboard from backend services",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}

internal sealed record CreateProductRequest(string Name, decimal Price);
internal sealed record UpdateProductRequest(string Name, decimal Price);
internal sealed record UpdatePriceRequest(decimal NewPrice);
internal sealed record RegisterUserRequest(string Email, string DisplayName);
internal sealed record CancelOrderRequest(string Reason);
internal sealed record CreateOrderRequest(Guid CustomerId, IReadOnlyList<CreateOrderLineRequest> Lines, Guid? CartId = null, Guid? OrderId = null);
internal sealed record CreateOrderLineRequest(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
internal sealed record UpdateOrderRequest(IReadOnlyList<CreateOrderLineRequest> Lines);
internal sealed record MarkOrderPaidRequest(Guid PaymentId, decimal Amount);
internal sealed record AdjustInventoryRequest(int OnHand);
internal sealed record CreateCartRequest(Guid CustomerId);
internal sealed record AddCartItemRequest(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
internal sealed record StartCheckoutSagaRequest(Guid CartId, bool SimulatePaymentFailure = false);

using CqrsDemo.Commands.Application;
using CqrsDemo.Commands.Application.Carts.Commands.AddCartItem;
using CqrsDemo.Commands.Application.Carts.Commands.CreateCart;
using CqrsDemo.Commands.Application.Carts.Commands.RemoveCartItem;
using CqrsDemo.Commands.Application.Orders.Commands.CheckoutCart;
using CqrsDemo.Commands.Application.Payments.Commands.PayOrder;
using CqrsDemo.Commands.Application.Products.Commands.CreateProduct;
using CqrsDemo.Commands.Application.Products.Commands.UpdateProductPrice;
using CqrsDemo.Commands.Application.Products.Queries.GetProductEventHistory;
using CqrsDemo.Commands.Infrastructure;
using CqrsDemo.Domain.Common;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCommandsApplication();
builder.Services.AddCommandsInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.MigrateCommandsDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    service = "CqrsDemo.Commands.Api",
    description = "Shopping cart write API (Event Sourcing + outbox + Azure Service Bus)",
    flow = new[] { "Create cart → Add items → Checkout → Pay order" },
    endpoints = new
    {
        products = new[] { "POST /api/products", "PUT /api/products/{id}/price", "GET /api/products/{id}/events" },
        carts = new[] { "POST /api/carts", "POST /api/carts/{id}/items", "DELETE /api/carts/{id}/items/{productId}", "POST /api/carts/{id}/checkout" },
        payments = new[] { "POST /api/orders/{orderId}/pay" }
    }
}));

app.MapPost("/api/products", async (CreateProductRequest request, IMediator mediator) =>
{
    var id = await mediator.Send(new CreateProductCommand(request.Name, request.Price));
    return Accepted(id, "/api/products", "Product");
});

app.MapPut("/api/products/{id:guid}/price", async (Guid id, UpdateProductPriceRequest request, IMediator mediator) =>
{
    return await ExecuteAsync(() => mediator.Send(new UpdateProductPriceCommand(id, request.NewPrice)),
        () => Results.Accepted($"/api/products/{id}", new { id, message = "Price update accepted." }));
});

app.MapGet("/api/products/{id:guid}/events", async (Guid id, IMediator mediator) =>
{
    var events = await mediator.Send(new GetProductEventHistoryQuery(id));
    return events is null ? Results.NotFound() : Results.Ok(events);
});

app.MapPost("/api/carts", async (CreateCartRequest request, IMediator mediator) =>
{
    var id = await mediator.Send(new CreateCartCommand(request.CustomerId));
    return Accepted(id, $"/api/carts/{id}", "Cart");
});

app.MapPost("/api/carts/{cartId:guid}/items", async (Guid cartId, AddCartItemRequest request, IMediator mediator) =>
{
    return await ExecuteAsync(
        () => mediator.Send(new AddCartItemCommand(
            cartId, request.ProductId, request.ProductName, request.UnitPrice, request.Quantity)),
        () => Results.Accepted($"/api/carts/{cartId}", new { cartId, message = "Item added." }));
});

app.MapDelete("/api/carts/{cartId:guid}/items/{productId:guid}", async (Guid cartId, Guid productId, IMediator mediator) =>
{
    return await ExecuteAsync(
        () => mediator.Send(new RemoveCartItemCommand(cartId, productId)),
        () => Results.Accepted($"/api/carts/{cartId}", new { cartId, productId, message = "Item removed." }));
});

app.MapPost("/api/carts/{cartId:guid}/checkout", async (Guid cartId, IMediator mediator) =>
{
    return await ExecuteWithResultAsync(
        () => mediator.Send(new CheckoutCartCommand(cartId)),
        orderId => Results.Accepted($"/api/orders/{orderId}", new { orderId, cartId, message = "Checkout accepted." }));
});

app.MapPost("/api/orders/{orderId:guid}/pay", async (Guid orderId, IMediator mediator) =>
{
    return await ExecuteWithResultAsync(
        () => mediator.Send(new PayOrderCommand(orderId)),
        paymentId => Results.Accepted($"/api/orders/{orderId}", new
        {
            orderId,
            paymentId,
            message = "Payment accepted."
        }));
});

app.Run();

static IResult Accepted(Guid id, string location, string entity) =>
    Results.Accepted(location, new { id, message = $"{entity} accepted. Read model updates asynchronously." });

static async Task<IResult> ExecuteAsync(Func<Task> action, Func<IResult> onSuccess)
{
    try
    {
        await action();
        return onSuccess();
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (ConcurrencyException ex)
    {
        return Results.Conflict(new
        {
            message = ex.Message,
            ex.StreamId,
            ex.ExpectedVersion,
            ex.ActualVersion
        });
    }
}

static async Task<IResult> ExecuteWithResultAsync<T>(Func<Task<T>> action, Func<T, IResult> onSuccess)
{
    try
    {
        var result = await action();
        return onSuccess(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (ConcurrencyException ex)
    {
        return Results.Conflict(new
        {
            message = ex.Message,
            ex.StreamId,
            ex.ExpectedVersion,
            ex.ActualVersion
        });
    }
}

internal sealed record CreateProductRequest(string Name, decimal Price);
internal sealed record UpdateProductPriceRequest(decimal NewPrice);
internal sealed record CreateCartRequest(Guid CustomerId);
internal sealed record AddCartItemRequest(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

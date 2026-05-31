using Cart.Application;
using Cart.Application.Commands.AddCartItem;
using Cart.Application.Commands.CheckoutCart;
using Cart.Application.Commands.CreateCart;
using Cart.Application.Commands.RemoveCartItem;
using Cart.Infrastructure;
using CqrsDemo.BuildingBlocks.Domain;
using MediatR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCartApplication();
builder.Services.AddCartWriteInfrastructure(builder.Configuration);

var app = builder.Build();
await app.Services.InitializeCartWriteStoreAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new { service = "Cart.Commands.Api", port = 5202 }));

app.MapPost("/api/carts", async (CreateCartRequest request, IMediator mediator) =>
{
    var id = await mediator.Send(new CreateCartCommand(request.CustomerId));
    return Accepted(id, $"/api/carts/{id}");
});

app.MapPost("/api/carts/{cartId:guid}/items", async (Guid cartId, AddCartItemRequest request, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new AddCartItemCommand(
        cartId, request.ProductId, request.ProductName, request.UnitPrice, request.Quantity)),
        () => Results.Accepted($"/api/carts/{cartId}", new { cartId })));

app.MapDelete("/api/carts/{cartId:guid}/items/{productId:guid}", async (Guid cartId, Guid productId, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new RemoveCartItemCommand(cartId, productId)),
        () => Results.Accepted($"/api/carts/{cartId}", new { cartId, productId })));

app.MapPost("/api/carts/{cartId:guid}/checkout", async (Guid cartId, IMediator mediator) =>
    await ExecuteWithResultAsync(() => mediator.Send(new CheckoutCartCommand(cartId)),
        orderId => Results.Accepted($"/api/orders/{orderId}", new { orderId, cartId })));

app.Run();

static IResult Accepted(Guid id, string location) =>
    Results.Accepted(location, new { id, message = "Accepted. Read model updates asynchronously." });

static async Task<IResult> ExecuteAsync(Func<Task> action, Func<IResult> onSuccess) =>
    await ExecuteWithResultAsync(async () => { await action(); return 0; }, _ => onSuccess());

static async Task<IResult> ExecuteWithResultAsync<T>(Func<Task<T>> action, Func<T, IResult> onSuccess)
{
    try
    {
        return onSuccess(await action());
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { message = ex.Message }); }
    catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
    catch (ConcurrencyException ex)
    {
        return Results.Conflict(new { message = ex.Message, ex.StreamId, ex.ExpectedVersion, ex.ActualVersion });
    }
}

internal sealed record CreateCartRequest(Guid CustomerId);
internal sealed record AddCartItemRequest(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

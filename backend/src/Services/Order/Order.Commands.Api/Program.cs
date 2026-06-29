using CqrsDemo.BuildingBlocks.Observability;
using MediatR;
using Order.Application;
using Order.Application.Commands.CancelOrder;
using Order.Application.Commands.CreateOrder;
using Order.Application.Commands.DeleteOrder;
using Order.Application.Commands.MarkOrderPaid;
using Order.Application.Commands.UpdateOrder;
using Order.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddPlatformObservability("order-commands");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOrderApplication();
builder.Services.AddOrderWriteInfrastructure(builder.Configuration);

var app = builder.Build();
app.UsePlatformObservability();
await app.Services.InitializeOrderWriteStoreAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    service = "Order.Commands.Api",
    port = 5203,
    note = "Orders can be created via admin API, cart checkout, or saga orchestration."
}));

app.MapPost("/api/orders", async (CreateOrderRequest request, IMediator mediator) =>
{
    try
    {
        var id = await mediator.Send(new CreateOrderCommand(
            request.CustomerId,
            request.Lines.Select(l => new OrderLineInput(l.ProductId, l.ProductName, l.UnitPrice, l.Quantity)).ToList(),
            request.CartId,
            request.OrderId));
        return Results.Accepted($"/api/orders/{id}", new { id });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { message = ex.Message }); }
    catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
});

app.MapPut("/api/orders/{orderId:guid}", async (Guid orderId, UpdateOrderRequest request, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new UpdateOrderCommand(
        orderId,
        request.Lines.Select(l => new UpdateOrderLineInput(l.ProductId, l.ProductName, l.UnitPrice, l.Quantity)).ToList()))));

app.MapDelete("/api/orders/{orderId:guid}", async (Guid orderId, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new DeleteOrderCommand(orderId))));

app.MapPost("/api/orders/{orderId:guid}/mark-paid", async (Guid orderId, MarkOrderPaidRequest request, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new MarkOrderPaidCommand(orderId, request.PaymentId, request.Amount))));

app.MapPost("/api/orders/{orderId:guid}/cancel", async (Guid orderId, CancelOrderRequest request, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new CancelOrderCommand(orderId, request.Reason))));

app.Run();

static async Task<IResult> ExecuteAsync(Func<Task> action)
{
    try
    {
        await action();
        return Results.NoContent();
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { message = ex.Message }); }
    catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
}

internal sealed record CreateOrderRequest(
    Guid CustomerId,
    IReadOnlyList<CreateOrderLineRequest> Lines,
    Guid? CartId = null,
    Guid? OrderId = null);

internal sealed record CreateOrderLineRequest(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
internal sealed record UpdateOrderRequest(IReadOnlyList<CreateOrderLineRequest> Lines);
internal sealed record MarkOrderPaidRequest(Guid PaymentId, decimal Amount);
internal sealed record CancelOrderRequest(string Reason);

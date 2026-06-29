using CqrsDemo.BuildingBlocks.Observability;
using Inventory.Application;
using Inventory.Application.Abstractions;
using Inventory.Application.Commands.AdjustInventory;
using Inventory.Application.Commands.ConfirmStockForOrder;
using Inventory.Application.Commands.InitializeInventory;
using Inventory.Application.Commands.ReleaseStockForOrder;
using Inventory.Application.Commands.ReserveStockForOrder;
using Inventory.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);
builder.AddPlatformObservability("inventory-commands");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInventoryApplication();
builder.Services.AddInventoryInfrastructure(builder.Configuration);

var app = builder.Build();
app.UsePlatformObservability();
await app.Services.InitializeInventoryStoreAsync();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.MapGet("/", () => Results.Ok(new { service = "Inventory.Commands.Api", port = 5208 }));

app.MapPost("/api/inventory/initialize", async (InitializeInventoryRequest request, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new InitializeInventoryCommand(request.ProductId, request.ProductName, request.InitialOnHand))));

app.MapPut("/api/inventory/{productId:guid}", async (Guid productId, AdjustInventoryRequest request, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new AdjustInventoryCommand(productId, request.OnHand))));

app.MapPost("/api/inventory/reserve", async (ReserveStockRequest request, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new ReserveStockForOrderCommand(
        request.OrderId,
        request.Lines.Select(l => new StockLineRequest(l.ProductId, l.Quantity)).ToList()))));

app.MapPost("/api/inventory/release/{orderId:guid}", async (Guid orderId, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new ReleaseStockForOrderCommand(orderId))));

app.MapPost("/api/inventory/confirm/{orderId:guid}", async (Guid orderId, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new ConfirmStockForOrderCommand(orderId))));

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

internal sealed record InitializeInventoryRequest(Guid ProductId, string ProductName, int InitialOnHand = 100);
internal sealed record AdjustInventoryRequest(int OnHand);
internal sealed record ReserveStockRequest(Guid OrderId, IReadOnlyList<ReserveStockLineRequest> Lines);
internal sealed record ReserveStockLineRequest(Guid ProductId, int Quantity);

using CqrsDemo.BuildingBlocks.Observability;
using Inventory.Application;
using Inventory.Application.Queries.GetAllInventory;
using Inventory.Application.Queries.GetInventoryByProductId;
using Inventory.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);
builder.AddPlatformObservability("inventory-queries");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInventoryApplication();
builder.Services.AddInventoryInfrastructure(builder.Configuration);

var app = builder.Build();
app.UsePlatformObservability();
await app.Services.InitializeInventoryStoreAsync();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.MapGet("/", () => Results.Ok(new { service = "Inventory.Queries.Api", port = 5218 }));
app.MapGet("/api/inventory", async (IMediator mediator) => Results.Ok(await mediator.Send(new GetAllInventoryQuery())));
app.MapGet("/api/inventory/{productId:guid}", async (Guid productId, IMediator mediator) =>
{
    var item = await mediator.Send(new GetInventoryByProductIdQuery(productId));
    return item is null ? Results.NotFound() : Results.Ok(item);
});

app.Run();

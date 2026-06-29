using CqrsDemo.BuildingBlocks.Observability;
using MediatR;
using Order.Application;
using Order.Application.Queries.GetAllOrders;
using Order.Application.Queries.GetOrderById;
using Order.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddPlatformObservability("order-queries");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOrderApplication();
builder.Services.AddOrderReadInfrastructure(builder.Configuration);

var app = builder.Build();
app.UsePlatformObservability();
await app.Services.InitializeOrderReadStoreAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new { service = "Order.Queries.Api", port = 5213 }));

app.MapGet("/api/orders", async (IMediator mediator) =>
    Results.Ok(await mediator.Send(new GetAllOrdersQuery())));

app.MapGet("/api/orders/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var order = await mediator.Send(new GetOrderByIdQuery(id));
    return order is null ? Results.NotFound() : Results.Ok(order);
});

app.Run();

using CqrsDemo.Queries.Application;
using CqrsDemo.Queries.Application.Carts.Queries.GetCartById;
using CqrsDemo.Queries.Application.Orders.Queries.GetAllOrders;
using CqrsDemo.Queries.Application.Orders.Queries.GetOrderById;
using CqrsDemo.Queries.Application.Payments.Queries.GetPaymentByOrderId;
using CqrsDemo.Queries.Application.Products.Queries.GetAllProducts;
using CqrsDemo.Queries.Application.Products.Queries.GetProductById;
using CqrsDemo.Queries.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddQueriesApplication();
builder.Services.AddQueriesInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.MigrateQueriesDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    service = "CqrsDemo.Queries.Api",
    description = "Shopping catalog & read models (eventual consistency)",
    endpoints = new
    {
        products = new[] { "GET /api/products", "GET /api/products/{id}" },
        carts = new[] { "GET /api/carts/{id}" },
        orders = new[] { "GET /api/orders", "GET /api/orders/{id}", "GET /api/orders/{id}/payment" }
    }
}));

app.MapGet("/api/products", async (IMediator mediator) =>
    Results.Ok(await mediator.Send(new GetAllProductsQuery())));

app.MapGet("/api/products/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var product = await mediator.Send(new GetProductByIdQuery(id));
    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.MapGet("/api/carts/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var cart = await mediator.Send(new GetCartByIdQuery(id));
    return cart is null ? Results.NotFound() : Results.Ok(cart);
});

app.MapGet("/api/orders", async (IMediator mediator) =>
    Results.Ok(await mediator.Send(new GetAllOrdersQuery())));

app.MapGet("/api/orders/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var order = await mediator.Send(new GetOrderByIdQuery(id));
    return order is null ? Results.NotFound() : Results.Ok(order);
});

app.MapGet("/api/orders/{orderId:guid}/payment", async (Guid orderId, IMediator mediator) =>
{
    var payment = await mediator.Send(new GetPaymentByOrderIdQuery(orderId));
    return payment is null ? Results.NotFound() : Results.Ok(payment);
});

app.Run();

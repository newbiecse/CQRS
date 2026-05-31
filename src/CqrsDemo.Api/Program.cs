using CqrsDemo.Application;
using CqrsDemo.Application.Products.Commands.CreateProduct;
using CqrsDemo.Application.Products.Commands.UpdateProductPrice;
using CqrsDemo.Application.Products.Queries.GetAllProducts;
using CqrsDemo.Application.Products.Queries.GetProductById;
using CqrsDemo.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.MigrateDatabasesAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    message = "CQRS + Event-Driven Demo API",
    endpoints = new[]
    {
        "POST /api/products",
        "PUT /api/products/{id}/price",
        "GET /api/products",
        "GET /api/products/{id}"
    }
}));

app.MapPost("/api/products", async (CreateProductRequest request, IMediator mediator) =>
{
    var productId = await mediator.Send(new CreateProductCommand(request.Name, request.Price));
    return Results.Created($"/api/products/{productId}", new { id = productId });
});

app.MapPut("/api/products/{id:guid}/price", async (Guid id, UpdateProductPriceRequest request, IMediator mediator) =>
{
    try
    {
        await mediator.Send(new UpdateProductPriceCommand(id, request.NewPrice));
        return Results.NoContent();
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
});

app.MapGet("/api/products", async (IMediator mediator) =>
{
    var products = await mediator.Send(new GetAllProductsQuery());
    return Results.Ok(products);
});

app.MapGet("/api/products/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var product = await mediator.Send(new GetProductByIdQuery(id));
    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.Run();

internal sealed record CreateProductRequest(string Name, decimal Price);
internal sealed record UpdateProductPriceRequest(decimal NewPrice);

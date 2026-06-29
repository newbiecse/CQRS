using MediatR;
using Product.Application;
using Product.Application.Commands.CreateProduct;
using Product.Application.Commands.UpdateProductPrice;
using Product.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProductApplication();
builder.Services.AddProductWriteInfrastructure(builder.Configuration);

var app = builder.Build();
await app.Services.InitializeProductWriteStoreAsync();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.MapGet("/", () => Results.Ok(new { service = "Product.Commands.Api", port = 5201 }));

app.MapPost("/api/products", async (CreateProductRequest r, IMediator m) =>
{
    var id = await m.Send(new CreateProductCommand(r.Name, r.Price));
    return Results.Accepted($"/api/products/{id}", new { id });
});

app.MapPut("/api/products/{id:guid}/price", async (Guid id, UpdatePriceRequest r, IMediator m) =>
{
    try
    {
        await m.Send(new UpdateProductPriceCommand(id, r.NewPrice));
        return Results.Accepted($"/api/products/{id}", new { id });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
});

app.Run();
internal sealed record CreateProductRequest(string Name, decimal Price);
internal sealed record UpdatePriceRequest(decimal NewPrice);

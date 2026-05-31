using MediatR;
using Product.Application;
using Product.Application.Queries.GetAllProducts;
using Product.Application.Queries.GetProductById;
using Product.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProductApplication();
builder.Services.AddProductReadInfrastructure(builder.Configuration);

var app = builder.Build();
await app.Services.InitializeProductReadStoreAsync();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.MapGet("/", () => Results.Ok(new { service = "Product.Queries.Api", port = 5211 }));
app.MapGet("/api/products", async (IMediator m) => Results.Ok(await m.Send(new GetAllProductsQuery())));
app.MapGet("/api/products/{id:guid}", async (Guid id, IMediator m) =>
{
    var p = await m.Send(new GetProductByIdQuery(id));
    return p is null ? Results.NotFound() : Results.Ok(p);
});
app.Run();

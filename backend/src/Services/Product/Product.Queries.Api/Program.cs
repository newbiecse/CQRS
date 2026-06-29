using CqrsDemo.BuildingBlocks.Observability;
using MediatR;
using Product.Application;
using Product.Application.Queries.GetAllProducts;
using Product.Application.Queries.GetProductById;
using Product.Application.Queries.SearchProducts;
using Product.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddPlatformObservability("product-queries");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProductApplication();
builder.Services.AddProductReadInfrastructure(builder.Configuration);
builder.Services.AddProductElasticsearchSearch(builder.Configuration);

var app = builder.Build();
app.UsePlatformObservability();
await app.Services.InitializeProductReadStoreAsync();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.MapGet("/", () => Results.Ok(new { service = "Product.Queries.Api", port = 5211 }));
app.MapGet("/api/products", async (IMediator m) => Results.Ok(await m.Send(new GetAllProductsQuery())));
app.MapGet("/api/products/search", async (string? q, int? size, IMediator m) =>
{
    if (string.IsNullOrWhiteSpace(q))
        return Results.BadRequest(new { message = "Query parameter 'q' is required." });

    try
    {
        var results = await m.Send(new SearchProductsQuery(q, size ?? 20));
        return Results.Ok(results);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(title: "Product search failed", detail: ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});
app.MapGet("/api/products/{id:guid}", async (Guid id, IMediator m) =>
{
    var p = await m.Send(new GetProductByIdQuery(id));
    return p is null ? Results.NotFound() : Results.Ok(p);
});
app.Run();

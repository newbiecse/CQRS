using Cart.Application;
using Cart.Application.Queries.GetCartById;
using Cart.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCartApplication();
builder.Services.AddCartReadInfrastructure(builder.Configuration);

var app = builder.Build();
await app.Services.InitializeCartReadStoreAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new { service = "Cart.Queries.Api", port = 5212 }));

app.MapGet("/api/carts/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var cart = await mediator.Send(new GetCartByIdQuery(id));
    return cart is null ? Results.NotFound() : Results.Ok(cart);
});

app.Run();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();

app.MapGet("/", () => Results.Ok(new
{
    service = "Shop.Gateway.Api",
    port = 5000,
    routes = new
    {
        productCommands = "/product-commands/api/products",
        productQueries = "/product-queries/api/products",
        cartCommands = "/cart-commands/api/carts",
        cartQueries = "/cart-queries/api/carts/{id}",
        orderQueries = "/order-queries/api/orders",
        paymentCommands = "/payment-commands/api/orders/{orderId}/pay"
    }
}));

app.MapReverseProxy();
app.Run();

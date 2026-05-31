var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

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

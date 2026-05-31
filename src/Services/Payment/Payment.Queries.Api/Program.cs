using MediatR;
using Payment.Application;
using Payment.Application.Queries.GetPaymentByOrderId;
using Payment.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddPaymentApplication();
builder.Services.AddPaymentReadInfrastructure(builder.Configuration);

var app = builder.Build();
await app.Services.InitializePaymentReadStoreAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new { service = "Payment.Queries.Api", port = 5214 }));

app.MapGet("/api/orders/{orderId:guid}/payment", async (Guid orderId, IMediator mediator) =>
{
    var payment = await mediator.Send(new GetPaymentByOrderIdQuery(orderId));
    return payment is null ? Results.NotFound() : Results.Ok(payment);
});

app.Run();

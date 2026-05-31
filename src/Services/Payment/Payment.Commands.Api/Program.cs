using CqrsDemo.BuildingBlocks.Domain;
using MediatR;
using Payment.Application;
using Payment.Application.Commands.PayOrder;
using Payment.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddPaymentApplication();
builder.Services.AddPaymentWriteInfrastructure(builder.Configuration);

var app = builder.Build();
await app.Services.InitializePaymentWriteStoreAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new { service = "Payment.Commands.Api", port = 5204 }));

app.MapPost("/api/orders/{orderId:guid}/pay", async (Guid orderId, bool simulateFailure, IMediator mediator) =>
{
    try
    {
        var paymentId = await mediator.Send(new PayOrderCommand(orderId, simulateFailure));
        return Results.Accepted($"/api/orders/{orderId}/payment", new { orderId, paymentId, simulateFailure });
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { message = ex.Message }); }
    catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
    catch (ConcurrencyException ex)
    {
        return Results.Conflict(new { message = ex.Message, ex.StreamId, ex.ExpectedVersion, ex.ActualVersion });
    }
});

app.Run();

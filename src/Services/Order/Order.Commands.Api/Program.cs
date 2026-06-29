using MediatR;
using Order.Application;
using Order.Application.Commands.CancelOrder;
using Order.Application.Commands.MarkOrderPaid;
using Order.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOrderApplication();
builder.Services.AddOrderWriteInfrastructure(builder.Configuration);

var app = builder.Build();
await app.Services.InitializeOrderWriteStoreAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    service = "Order.Commands.Api",
    port = 5203,
    note = "Orders are created via cart.checked-out integration events. Saga orchestrator marks paid or cancels."
}));

app.MapPost("/api/orders/{orderId:guid}/mark-paid", async (Guid orderId, MarkOrderPaidRequest request, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new MarkOrderPaidCommand(orderId, request.PaymentId, request.Amount))));

app.MapPost("/api/orders/{orderId:guid}/cancel", async (Guid orderId, CancelOrderRequest request, IMediator mediator) =>
    await ExecuteAsync(() => mediator.Send(new CancelOrderCommand(orderId, request.Reason))));

app.Run();

static async Task<IResult> ExecuteAsync(Func<Task> action)
{
    try
    {
        await action();
        return Results.NoContent();
    }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { message = ex.Message }); }
    catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
}

internal sealed record MarkOrderPaidRequest(Guid PaymentId, decimal Amount);
internal sealed record CancelOrderRequest(string Reason);

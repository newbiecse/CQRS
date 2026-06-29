using CqrsDemo.BuildingBlocks.Observability;
using CheckoutSaga.Application;
using CheckoutSaga.Application.Commands.StartCheckoutSaga;
using CheckoutSaga.Application.Queries.GetCheckoutSaga;
using CheckoutSaga.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);
builder.AddPlatformObservability("checkout-saga-api");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCheckoutSagaApplication();
builder.Services.AddCheckoutSagaInfrastructure(builder.Configuration);

var app = builder.Build();
app.UsePlatformObservability();
await app.Services.InitializeCheckoutSagaStoreAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    service = "CheckoutSaga.Api",
    port = 5205,
    pattern = "Saga orchestration for distributed checkout (cart → order → payment → compensate)"
}));

app.MapPost("/api/sagas/checkout", async (StartCheckoutSagaRequest request, IMediator mediator) =>
{
    try
    {
        var result = await mediator.Send(new StartCheckoutSagaCommand(request.CartId, request.SimulatePaymentFailure));
        return Results.Accepted($"/api/sagas/{result.SagaId}", result);
    }
    catch (InvalidOperationException ex) { return Results.Conflict(new { message = ex.Message }); }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
});

app.MapGet("/api/sagas/{sagaId:guid}", async (Guid sagaId, IMediator mediator) =>
{
    var saga = await mediator.Send(new GetCheckoutSagaQuery(sagaId));
    return saga is null ? Results.NotFound() : Results.Ok(saga);
});

app.Run();

internal sealed record StartCheckoutSagaRequest(Guid CartId, bool SimulatePaymentFailure = false);

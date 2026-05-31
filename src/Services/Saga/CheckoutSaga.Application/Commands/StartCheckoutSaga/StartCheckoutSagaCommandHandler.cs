using CheckoutSaga.Application.Orchestration;
using MediatR;

namespace CheckoutSaga.Application.Commands.StartCheckoutSaga;

public sealed class StartCheckoutSagaCommandHandler(CheckoutSagaOrchestrator orchestrator)
    : IRequestHandler<StartCheckoutSagaCommand, StartCheckoutSagaResult>
{
    public async Task<StartCheckoutSagaResult> Handle(StartCheckoutSagaCommand request, CancellationToken cancellationToken)
    {
        var saga = await orchestrator.StartAsync(request.CartId, request.SimulatePaymentFailure, cancellationToken);
        return new StartCheckoutSagaResult(saga.Id, saga.CartId, saga.OrderId, saga.State);
    }
}

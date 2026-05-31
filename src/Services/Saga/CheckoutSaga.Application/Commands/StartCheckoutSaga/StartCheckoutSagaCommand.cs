using MediatR;

namespace CheckoutSaga.Application.Commands.StartCheckoutSaga;

public sealed record StartCheckoutSagaCommand(Guid CartId, bool SimulatePaymentFailure = false)
    : IRequest<StartCheckoutSagaResult>;

public sealed record StartCheckoutSagaResult(
    Guid SagaId,
    Guid CartId,
    Guid? OrderId,
    string State);

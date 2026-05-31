using MediatR;

namespace Payment.Application.Commands.PayOrder;

public sealed record PayOrderCommand(Guid OrderId, bool SimulateFailure = false) : IRequest<Guid>;

using MediatR;

namespace CqrsDemo.Commands.Application.Payments.Commands.PayOrder;

public sealed record PayOrderCommand(Guid OrderId) : IRequest<Guid>;

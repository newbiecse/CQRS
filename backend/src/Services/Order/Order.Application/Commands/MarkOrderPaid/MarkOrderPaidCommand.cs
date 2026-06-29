using MediatR;

namespace Order.Application.Commands.MarkOrderPaid;

public sealed record MarkOrderPaidCommand(Guid OrderId, Guid PaymentId, decimal Amount) : IRequest;

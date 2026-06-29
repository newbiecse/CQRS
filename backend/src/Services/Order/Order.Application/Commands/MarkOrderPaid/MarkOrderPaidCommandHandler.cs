using MediatR;
using Order.Application.Abstractions;
using Order.Domain;

namespace Order.Application.Commands.MarkOrderPaid;

public sealed class MarkOrderPaidCommandHandler(IOrderWriteRepository repository)
    : IRequestHandler<MarkOrderPaidCommand>
{
    public async Task Handle(MarkOrderPaidCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        order.MarkAsPaid(request.PaymentId, request.Amount);
        await repository.UpdateAsync(order, cancellationToken);
    }
}

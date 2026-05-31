using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using MediatR;
using Order.Domain;

namespace Order.Application.Commands.MarkOrderPaid;

public sealed class MarkOrderPaidCommandHandler(IEventStore eventStore)
    : IRequestHandler<MarkOrderPaidCommand>
{
    public async Task Handle(MarkOrderPaidCommand request, CancellationToken cancellationToken)
    {
        var order = await eventStore.LoadAsync(
            request.OrderId,
            OrderAggregate.StreamType,
            OrderAggregate.Load,
            cancellationToken) ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        order.MarkAsPaid(request.PaymentId, request.Amount);
        await eventStore.SaveAsync(order, OrderAggregate.StreamType, cancellationToken);
    }
}

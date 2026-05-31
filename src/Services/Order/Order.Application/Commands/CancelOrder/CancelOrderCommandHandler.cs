using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using MediatR;
using Order.Domain;

namespace Order.Application.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler(IEventStore eventStore)
    : IRequestHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await eventStore.LoadAsync(
            request.OrderId,
            OrderAggregate.StreamType,
            OrderAggregate.Load,
            cancellationToken) ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        order.Cancel(request.Reason);
        await eventStore.SaveAsync(order, OrderAggregate.StreamType, cancellationToken);
    }
}

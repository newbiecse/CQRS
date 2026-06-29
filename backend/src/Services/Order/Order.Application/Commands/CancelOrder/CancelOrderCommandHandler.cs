using MediatR;
using Order.Application.Abstractions;
using Order.Domain;

namespace Order.Application.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler(
    IOrderWriteRepository repository,
    IInventoryCommandClient inventoryClient)
    : IRequestHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        order.Cancel(request.Reason);
        await repository.UpdateAsync(order, cancellationToken);
        await inventoryClient.ReleaseAsync(request.OrderId, cancellationToken);
    }
}

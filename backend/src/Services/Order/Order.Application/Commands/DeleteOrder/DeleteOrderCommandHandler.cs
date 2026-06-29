using MediatR;
using Order.Application.Abstractions;
using Order.Domain;

namespace Order.Application.Commands.DeleteOrder;

public sealed class DeleteOrderCommandHandler(
    IOrderWriteRepository repository,
    IInventoryCommandClient inventoryClient)
    : IRequestHandler<DeleteOrderCommand>
{
    public async Task Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        if (order.Status == OrderStatus.Paid)
            throw new InvalidOperationException("Paid orders cannot be deleted.");

        if (order.Status != OrderStatus.Cancelled)
        {
            order.Cancel(request.Reason);
            await repository.UpdateAsync(order, cancellationToken);
        }

        await inventoryClient.ReleaseAsync(request.OrderId, cancellationToken);
    }
}

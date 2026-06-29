using Inventory.Application.Abstractions;
using MediatR;

namespace Inventory.Application.Commands.ConfirmStockForOrder;

public sealed class ConfirmStockForOrderCommandHandler(IInventoryRepository repository)
    : IRequestHandler<ConfirmStockForOrderCommand>
{
    public async Task Handle(ConfirmStockForOrderCommand request, CancellationToken cancellationToken)
    {
        if (!await repository.HasReservationForOrderAsync(request.OrderId, cancellationToken))
            return;

        await repository.ConfirmForOrderAsync(request.OrderId, cancellationToken);
    }
}

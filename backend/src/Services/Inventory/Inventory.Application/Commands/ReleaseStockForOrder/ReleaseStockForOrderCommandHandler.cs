using Inventory.Application.Abstractions;
using MediatR;

namespace Inventory.Application.Commands.ReleaseStockForOrder;

public sealed class ReleaseStockForOrderCommandHandler(IInventoryRepository repository)
    : IRequestHandler<ReleaseStockForOrderCommand>
{
    public async Task Handle(ReleaseStockForOrderCommand request, CancellationToken cancellationToken)
    {
        if (!await repository.HasReservationForOrderAsync(request.OrderId, cancellationToken))
            return;

        await repository.ReleaseForOrderAsync(request.OrderId, cancellationToken);
    }
}

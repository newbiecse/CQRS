using Inventory.Application.Abstractions;
using MediatR;

namespace Inventory.Application.Commands.ReserveStockForOrder;

public sealed class ReserveStockForOrderCommandHandler(IInventoryRepository repository)
    : IRequestHandler<ReserveStockForOrderCommand>
{
    public async Task Handle(ReserveStockForOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Lines.Count == 0)
            throw new ArgumentException("At least one line is required.", nameof(request.Lines));

        await repository.ReserveForOrderAsync(request.OrderId, request.Lines, cancellationToken);
    }
}

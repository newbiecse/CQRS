using Inventory.Application.Abstractions;
using MediatR;

namespace Inventory.Application.Commands.AdjustInventory;

public sealed class AdjustInventoryCommandHandler(IInventoryRepository repository)
    : IRequestHandler<AdjustInventoryCommand>
{
    public async Task Handle(AdjustInventoryCommand request, CancellationToken cancellationToken)
    {
        var item = await repository.GetByProductIdAsync(request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"Inventory for product {request.ProductId} was not found.");

        item.AdjustOnHand(request.OnHand);
        await repository.UpdateAsync(item, cancellationToken);
    }
}

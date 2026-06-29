using Inventory.Application.Abstractions;
using Inventory.Domain;
using MediatR;

namespace Inventory.Application.Commands.InitializeInventory;

public sealed class InitializeInventoryCommandHandler(IInventoryRepository repository)
    : IRequestHandler<InitializeInventoryCommand>
{
    public async Task Handle(InitializeInventoryCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByProductIdAsync(request.ProductId, cancellationToken);
        if (existing is not null) return;

        var item = InventoryItem.Initialize(request.ProductId, request.ProductName, request.InitialOnHand);
        await repository.InitializeAsync(item, cancellationToken);
    }
}

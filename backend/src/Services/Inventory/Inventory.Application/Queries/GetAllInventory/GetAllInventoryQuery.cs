using Inventory.Application.Abstractions;
using Inventory.Application.ReadModels;
using MediatR;

namespace Inventory.Application.Queries.GetAllInventory;

public sealed record GetAllInventoryQuery : IRequest<IReadOnlyList<InventoryReadModel>>;

public sealed class GetAllInventoryQueryHandler(IInventoryRepository repository)
    : IRequestHandler<GetAllInventoryQuery, IReadOnlyList<InventoryReadModel>>
{
    public async Task<IReadOnlyList<InventoryReadModel>> Handle(GetAllInventoryQuery request, CancellationToken cancellationToken)
    {
        var items = await repository.GetAllAsync(cancellationToken);
        return items.Select(ToReadModel).ToList();
    }

    private static InventoryReadModel ToReadModel(Domain.InventoryItem item) =>
        new()
        {
            ProductId = item.Id,
            ProductName = item.ProductName,
            OnHand = item.OnHand,
            Reserved = item.Reserved,
            Available = item.Available,
            LastUpdatedAt = item.LastUpdatedAt
        };
}

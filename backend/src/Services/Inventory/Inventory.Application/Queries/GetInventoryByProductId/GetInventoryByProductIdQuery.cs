using Inventory.Application.Abstractions;
using Inventory.Application.ReadModels;
using MediatR;

namespace Inventory.Application.Queries.GetInventoryByProductId;

public sealed record GetInventoryByProductIdQuery(Guid ProductId) : IRequest<InventoryReadModel?>;

public sealed class GetInventoryByProductIdQueryHandler(IInventoryRepository repository)
    : IRequestHandler<GetInventoryByProductIdQuery, InventoryReadModel?>
{
    public async Task<InventoryReadModel?> Handle(GetInventoryByProductIdQuery request, CancellationToken cancellationToken)
    {
        var item = await repository.GetByProductIdAsync(request.ProductId, cancellationToken);
        return item is null
            ? null
            : new InventoryReadModel
            {
                ProductId = item.Id,
                ProductName = item.ProductName,
                OnHand = item.OnHand,
                Reserved = item.Reserved,
                Available = item.Available,
                LastUpdatedAt = item.LastUpdatedAt
            };
    }
}

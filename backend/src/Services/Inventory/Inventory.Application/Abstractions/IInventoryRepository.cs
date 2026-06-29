using Inventory.Domain;

namespace Inventory.Application.Abstractions;

public sealed record StockLineRequest(Guid ProductId, int Quantity);

public sealed record OrderReservationLine(Guid ProductId, int Quantity);

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> HasReservationForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderReservationLine>> GetReservationsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task InitializeAsync(InventoryItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(InventoryItem item, CancellationToken cancellationToken = default);
    Task ReserveForOrderAsync(Guid orderId, IReadOnlyList<StockLineRequest> lines, CancellationToken cancellationToken = default);
    Task ReleaseForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task ConfirmForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task DeleteByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}

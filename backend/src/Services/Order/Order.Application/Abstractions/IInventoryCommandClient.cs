namespace Order.Application.Abstractions;

public sealed record InventoryStockLine(Guid ProductId, int Quantity);

public interface IInventoryCommandClient
{
    Task ReserveAsync(Guid orderId, IReadOnlyList<InventoryStockLine> lines, CancellationToken cancellationToken = default);
    Task ReleaseAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task ConfirmAsync(Guid orderId, CancellationToken cancellationToken = default);
}

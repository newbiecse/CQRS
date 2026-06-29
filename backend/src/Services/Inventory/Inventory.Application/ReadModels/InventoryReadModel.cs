namespace Inventory.Application.ReadModels;

public sealed class InventoryReadModel
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int OnHand { get; init; }
    public int Reserved { get; init; }
    public int Available { get; init; }
    public DateTime LastUpdatedAt { get; init; }
}

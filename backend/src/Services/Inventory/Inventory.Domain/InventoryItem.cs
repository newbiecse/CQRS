using CqrsDemo.BuildingBlocks.Domain;

namespace Inventory.Domain;

public sealed class InventoryItem : AggregateRoot
{
    public string ProductName { get; private set; } = string.Empty;
    public int OnHand { get; private set; }
    public int Reserved { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    public int Available => OnHand - Reserved;

    private InventoryItem() { }

    public static InventoryItem Initialize(Guid productId, string productName, int initialOnHand)
    {
        if (productId == Guid.Empty) throw new ArgumentException("Product id is required.", nameof(productId));
        if (string.IsNullOrWhiteSpace(productName)) throw new ArgumentException("Product name is required.", nameof(productName));
        if (initialOnHand < 0) throw new ArgumentException("Initial on-hand quantity cannot be negative.", nameof(initialOnHand));

        return new InventoryItem
        {
            Id = productId,
            ProductName = productName.Trim(),
            OnHand = initialOnHand,
            Reserved = 0,
            LastUpdatedAt = DateTime.UtcNow
        };
    }

    public static InventoryItem Restore(Guid productId, string productName, int onHand, int reserved, DateTime lastUpdatedAt) =>
        new()
        {
            Id = productId,
            ProductName = productName,
            OnHand = onHand,
            Reserved = reserved,
            LastUpdatedAt = lastUpdatedAt
        };

    public void Rename(string productName)
    {
        if (string.IsNullOrWhiteSpace(productName)) throw new ArgumentException("Product name is required.", nameof(productName));
        ProductName = productName.Trim();
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void AdjustOnHand(int newOnHand)
    {
        if (newOnHand < Reserved)
            throw new InvalidOperationException($"On-hand ({newOnHand}) cannot be less than reserved ({Reserved}).");

        OnHand = newOnHand;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (Available < quantity)
            throw new InvalidOperationException($"Insufficient stock for product {ProductName}. Available: {Available}, requested: {quantity}.");

        Reserved += quantity;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (Reserved < quantity)
            throw new InvalidOperationException($"Cannot release {quantity}; only {Reserved} reserved.");

        Reserved -= quantity;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void Confirm(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (Reserved < quantity)
            throw new InvalidOperationException($"Cannot confirm {quantity}; only {Reserved} reserved.");
        if (OnHand < quantity)
            throw new InvalidOperationException($"Cannot confirm {quantity}; only {OnHand} on hand.");

        Reserved -= quantity;
        OnHand -= quantity;
        LastUpdatedAt = DateTime.UtcNow;
    }
}

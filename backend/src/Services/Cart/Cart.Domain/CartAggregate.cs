using Cart.Domain.Events;
using CqrsDemo.BuildingBlocks.Domain;

namespace Cart.Domain;

public sealed class CartAggregate : AggregateRoot
{
    public Guid CustomerId { get; private set; }
    public CartStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private readonly List<OrderLine> _items = [];
    public IReadOnlyList<OrderLine> Items => _items.AsReadOnly();
    public decimal Subtotal => _items.Sum(i => i.LineTotal);

    private CartAggregate() { }

    public static CartAggregate Create(Guid customerId)
    {
        var cart = new CartAggregate
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        cart.RaiseDomainEvent(new CartCreatedEvent(cart.Id, cart.CustomerId, cart.CreatedAt));
        return cart;
    }

    public static CartAggregate Restore(
        Guid id,
        Guid customerId,
        CartStatus status,
        DateTime createdAt,
        IReadOnlyList<OrderLine> items)
    {
        var cart = new CartAggregate
        {
            Id = id,
            CustomerId = customerId,
            Status = status,
            CreatedAt = createdAt
        };
        cart._items.AddRange(items);
        return cart;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        EnsureActive();
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0) throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        var trimmedName = productName.Trim();
        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is null)
        {
            _items.Add(new OrderLine
            {
                ProductId = productId,
                ProductName = trimmedName,
                UnitPrice = unitPrice,
                Quantity = quantity
            });
        }
        else
        {
            _items.Remove(existing);
            _items.Add(new OrderLine
            {
                ProductId = productId,
                ProductName = trimmedName,
                UnitPrice = unitPrice,
                Quantity = existing.Quantity + quantity
            });
        }

        RaiseDomainEvent(new CartItemAddedEvent(Id, productId, trimmedName, unitPrice, quantity));
    }

    public void RemoveItem(Guid productId)
    {
        EnsureActive();
        if (!_items.Any(i => i.ProductId == productId))
            throw new InvalidOperationException($"Product {productId} is not in the cart.");

        _items.RemoveAll(i => i.ProductId == productId);
        RaiseDomainEvent(new CartItemRemovedEvent(Id, productId));
    }

    public void Checkout(Guid orderId)
    {
        EnsureActive();
        if (_items.Count == 0) throw new InvalidOperationException("Cannot checkout an empty cart.");

        Status = CartStatus.CheckedOut;
        RaiseDomainEvent(new CartCheckedOutEvent(Id, orderId, CustomerId, _items.ToList(), Subtotal, DateTime.UtcNow));
    }

    private void EnsureActive()
    {
        if (Status != CartStatus.Active) throw new InvalidOperationException("Cart is no longer active.");
    }
}

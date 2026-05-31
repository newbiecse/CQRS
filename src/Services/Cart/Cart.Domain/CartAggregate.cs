using Cart.Domain.Events;
using CqrsDemo.BuildingBlocks.Domain;

namespace Cart.Domain;

public sealed class CartAggregate : AggregateRoot
{
    public const string StreamType = "Cart";

    public Guid CustomerId { get; private set; }
    public CartStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private readonly List<OrderLine> _items = [];
    public IReadOnlyList<OrderLine> Items => _items.AsReadOnly();
    public decimal Subtotal => _items.Sum(i => i.LineTotal);

    private CartAggregate() { }

    public static CartAggregate Create(Guid customerId)
    {
        var cart = new CartAggregate();
        cart.Raise(new CartCreatedEvent(Guid.NewGuid(), customerId, DateTime.UtcNow));
        return cart;
    }

    public static CartAggregate Load(IReadOnlyList<IDomainEvent> history)
    {
        var cart = new CartAggregate();
        foreach (var e in history) cart.Apply(e);
        cart.SetVersion(history.Count);
        cart.ClearDomainEvents();
        return cart;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        EnsureActive();
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0) throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        Raise(new CartItemAddedEvent(Id, productId, productName.Trim(), unitPrice, quantity));
    }

    public void RemoveItem(Guid productId)
    {
        EnsureActive();
        if (!_items.Any(i => i.ProductId == productId))
            throw new InvalidOperationException($"Product {productId} is not in the cart.");
        Raise(new CartItemRemovedEvent(Id, productId));
    }

    public void Checkout(Guid orderId)
    {
        EnsureActive();
        if (_items.Count == 0) throw new InvalidOperationException("Cannot checkout an empty cart.");
        Raise(new CartCheckedOutEvent(Id, orderId, CustomerId, _items.ToList(), Subtotal, DateTime.UtcNow));
    }

    private void EnsureActive()
    {
        if (Status != CartStatus.Active) throw new InvalidOperationException("Cart is no longer active.");
    }

    private void Raise(IDomainEvent e)
    {
        Apply(e);
        RaiseDomainEvent(e);
    }

    private void Apply(IDomainEvent e)
    {
        switch (e)
        {
            case CartCreatedEvent created:
                ApplyCreated(created);
                break;
            case CartItemAddedEvent added:
                ApplyItemAdded(added);
                break;
            case CartItemRemovedEvent removed:
                ApplyItemRemoved(removed);
                break;
            case CartCheckedOutEvent checkedOut:
                ApplyCheckedOut(checkedOut);
                break;
            default:
                throw new NotSupportedException(e.GetType().Name);
        }
    }

    private void ApplyCreated(CartCreatedEvent created)
    {
        Id = created.CartId;
        CustomerId = created.CustomerId;
        CreatedAt = created.CreatedAt;
        Status = CartStatus.Active;
    }

    private void ApplyItemAdded(CartItemAddedEvent added)
    {
        var existing = _items.FirstOrDefault(i => i.ProductId == added.ProductId);
        if (existing is null)
        {
            _items.Add(new OrderLine
            {
                ProductId = added.ProductId,
                ProductName = added.ProductName,
                UnitPrice = added.UnitPrice,
                Quantity = added.Quantity
            });
        }
        else
        {
            _items.Remove(existing);
            _items.Add(new OrderLine
            {
                ProductId = added.ProductId,
                ProductName = added.ProductName,
                UnitPrice = added.UnitPrice,
                Quantity = existing.Quantity + added.Quantity
            });
        }
    }

    private void ApplyItemRemoved(CartItemRemovedEvent removed) =>
        _items.RemoveAll(i => i.ProductId == removed.ProductId);

    private void ApplyCheckedOut(CartCheckedOutEvent _) => Status = CartStatus.CheckedOut;
}

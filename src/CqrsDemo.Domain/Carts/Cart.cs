using CqrsDemo.Domain.Common;
using CqrsDemo.Domain.Carts.Events;

namespace CqrsDemo.Domain.Carts;

public class Cart : AggregateRoot
{
    public const string StreamType = "Cart";

    public Guid CustomerId { get; private set; }
    public CartStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private readonly List<OrderLine> _items = [];
    public IReadOnlyList<OrderLine> Items => _items.AsReadOnly();

    public decimal Subtotal => _items.Sum(i => i.LineTotal);

    private Cart()
    {
    }

    public static Cart Create(Guid customerId)
    {
        var cart = new Cart();
        cart.ApplyNew(new CartCreatedEvent(Guid.NewGuid(), customerId, DateTime.UtcNow));
        return cart;
    }

    public static Cart LoadFromHistory(IReadOnlyList<IDomainEvent> history)
    {
        var cart = new Cart();
        foreach (var domainEvent in history)
        {
            cart.Apply(domainEvent);
        }

        cart.SetVersion(history.Count);
        cart.ClearDomainEvents();
        return cart;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        EnsureActive();

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (unitPrice < 0)
        {
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        }

        ApplyNew(new CartItemAddedEvent(Id, productId, productName.Trim(), unitPrice, quantity));
    }

    public void RemoveItem(Guid productId)
    {
        EnsureActive();

        if (!_items.Any(i => i.ProductId == productId))
        {
            throw new InvalidOperationException($"Product {productId} is not in the cart.");
        }

        ApplyNew(new CartItemRemovedEvent(Id, productId));
    }

    public void Checkout(Guid orderId)
    {
        EnsureActive();

        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Cannot checkout an empty cart.");
        }

        ApplyNew(new CartCheckedOutEvent(Id, orderId, CustomerId, _items.ToList(), Subtotal, DateTime.UtcNow));
    }

    private void EnsureActive()
    {
        if (Status != CartStatus.Active)
        {
            throw new InvalidOperationException("Cart is no longer active.");
        }
    }

    private void ApplyNew(IDomainEvent domainEvent)
    {
        Apply(domainEvent);
        RaiseDomainEvent(domainEvent);
    }

    internal void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case CartCreatedEvent created:
                Id = created.CartId;
                CustomerId = created.CustomerId;
                CreatedAt = created.CreatedAt;
                Status = CartStatus.Active;
                break;

            case CartItemAddedEvent added:
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

                break;

            case CartItemRemovedEvent removed:
                _items.RemoveAll(i => i.ProductId == removed.ProductId);
                break;

            case CartCheckedOutEvent:
                Status = CartStatus.CheckedOut;
                break;

            default:
                throw new NotSupportedException(
                    $"Domain event {domainEvent.GetType().Name} is not supported by {nameof(Cart)}.");
        }
    }
}

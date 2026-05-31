using CqrsDemo.BuildingBlocks.Domain;
using Product.Domain.Events;

namespace Product.Domain;

public sealed class ProductAggregate : AggregateRoot
{
    public const string StreamType = "Product";
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ProductAggregate() { }

    public static ProductAggregate Create(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.");
        if (price < 0) throw new ArgumentException("Price invalid.");
        var p = new ProductAggregate();
        p.Raise(new ProductCreatedEvent(Guid.NewGuid(), name.Trim(), price, DateTime.UtcNow));
        return p;
    }

    public static ProductAggregate Load(IReadOnlyList<IDomainEvent> history)
    {
        var p = new ProductAggregate();
        foreach (var e in history) p.Apply(e);
        p.SetVersion(history.Count);
        p.ClearDomainEvents();
        return p;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0) throw new ArgumentException("Price invalid.");
        Raise(new ProductPriceUpdatedEvent(Id, Price, newPrice, DateTime.UtcNow));
    }

    private void Raise(IDomainEvent e) { Apply(e); RaiseDomainEvent(e); }

    private void Apply(IDomainEvent e)
    {
        switch (e)
        {
            case ProductCreatedEvent c:
                ApplyCreated(c);
                break;
            case ProductPriceUpdatedEvent u:
                Price = u.NewPrice;
                break;
            default:
                throw new NotSupportedException(e.GetType().Name);
        }
    }

    private void ApplyCreated(ProductCreatedEvent c)
    {
        Id = c.ProductId; Name = c.Name; Price = c.Price; CreatedAt = c.CreatedAt;
    }
}

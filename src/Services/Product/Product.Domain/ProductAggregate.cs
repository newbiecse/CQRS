using CqrsDemo.BuildingBlocks.Domain;
using Product.Domain.Events;

namespace Product.Domain;

public sealed class ProductAggregate : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ProductAggregate() { }

    public static ProductAggregate Create(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.");
        if (price < 0) throw new ArgumentException("Price invalid.");

        var product = new ProductAggregate
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Price = price,
            CreatedAt = DateTime.UtcNow
        };
        product.RaiseDomainEvent(new ProductCreatedEvent(product.Id, product.Name, product.Price, product.CreatedAt));
        return product;
    }

    public static ProductAggregate Restore(Guid id, string name, decimal price, DateTime createdAt) =>
        new()
        {
            Id = id,
            Name = name,
            Price = price,
            CreatedAt = createdAt
        };

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0) throw new ArgumentException("Price invalid.");
        var oldPrice = Price;
        Price = newPrice;
        RaiseDomainEvent(new ProductPriceUpdatedEvent(Id, oldPrice, newPrice, DateTime.UtcNow));
    }
}

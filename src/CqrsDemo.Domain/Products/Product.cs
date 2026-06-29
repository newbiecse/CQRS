using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.Domain.Products.Events;

namespace CqrsDemo.Domain.Products;

public sealed class Product : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Product()
    {
    }

    public static Product Create(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        if (price < 0)
        {
            throw new ArgumentException("Price cannot be negative.", nameof(price));
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Price = price,
            CreatedAt = DateTime.UtcNow
        };
        product.RaiseDomainEvent(new ProductCreatedEvent(product.Id, product.Name, product.Price, product.CreatedAt));
        return product;
    }

    public static Product Restore(Guid id, string name, decimal price, DateTime createdAt) =>
        new()
        {
            Id = id,
            Name = name,
            Price = price,
            CreatedAt = createdAt
        };

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
        {
            throw new ArgumentException("Price cannot be negative.", nameof(newPrice));
        }

        var oldPrice = Price;
        Price = newPrice;
        RaiseDomainEvent(new ProductPriceUpdatedEvent(Id, oldPrice, newPrice, DateTime.UtcNow));
    }
}

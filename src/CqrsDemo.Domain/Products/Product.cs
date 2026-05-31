using CqrsDemo.Domain.Common;
using CqrsDemo.Domain.Products.Events;

namespace CqrsDemo.Domain.Products;

public class Product : AggregateRoot
{
    public const string StreamType = "Product";

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

        var product = new Product();
        var domainEvent = new ProductCreatedEvent(
            Guid.NewGuid(),
            name.Trim(),
            price,
            DateTime.UtcNow);

        product.ApplyNew(domainEvent);
        return product;
    }

    public static Product LoadFromHistory(IReadOnlyList<IDomainEvent> history)
    {
        var product = new Product();

        foreach (var domainEvent in history)
        {
            product.Apply(domainEvent);
        }

        product.SetVersion(history.Count);
        product.ClearDomainEvents();
        return product;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
        {
            throw new ArgumentException("Price cannot be negative.", nameof(newPrice));
        }

        var domainEvent = new ProductPriceUpdatedEvent(Id, Price, newPrice, DateTime.UtcNow);
        ApplyNew(domainEvent);
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
            case ProductCreatedEvent created:
                Id = created.ProductId;
                Name = created.Name;
                Price = created.Price;
                CreatedAt = created.CreatedAt;
                break;

            case ProductPriceUpdatedEvent updated:
                Price = updated.NewPrice;
                break;

            default:
                throw new NotSupportedException(
                    $"Domain event {domainEvent.GetType().Name} is not supported by {nameof(Product)}.");
        }
    }
}

namespace CqrsDemo.Queries.Infrastructure.Persistence.Read;

public sealed class CartReadEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public ICollection<CartLineReadEntity> Lines { get; set; } = [];
}

public sealed class CartLineReadEntity
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public CartReadEntity? Cart { get; set; }
}

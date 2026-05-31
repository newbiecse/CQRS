namespace CqrsDemo.Queries.Infrastructure.Persistence.Read;

public sealed class OrderReadEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid CartId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public Guid? PaymentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public ICollection<OrderLineReadEntity> Lines { get; set; } = [];
}

public sealed class OrderLineReadEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public OrderReadEntity? Order { get; set; }
}

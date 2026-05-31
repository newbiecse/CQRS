namespace CqrsDemo.Domain.Common;

public sealed class OrderLine
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }

    public decimal LineTotal => UnitPrice * Quantity;
}

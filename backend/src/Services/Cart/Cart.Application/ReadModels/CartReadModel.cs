using CqrsDemo.Contracts.Common;

namespace Cart.Application.ReadModels;

public sealed class CartReadModel
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<OrderLineDto> Items { get; init; } = [];
    public decimal Subtotal { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastUpdatedAt { get; init; }
}

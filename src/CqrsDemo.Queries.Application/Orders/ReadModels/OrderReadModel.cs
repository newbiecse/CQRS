using CqrsDemo.Contracts.Common;

namespace CqrsDemo.Queries.Application.Orders.ReadModels;

public sealed class OrderReadModel
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public Guid CartId { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<OrderLineDto> Lines { get; init; } = [];
    public decimal TotalAmount { get; init; }
    public Guid? PaymentId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastUpdatedAt { get; init; }
}

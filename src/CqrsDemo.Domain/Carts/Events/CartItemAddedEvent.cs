using CqrsDemo.BuildingBlocks.Domain;

namespace CqrsDemo.Domain.Carts.Events;

public sealed record CartItemAddedEvent(
    Guid CartId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

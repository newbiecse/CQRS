using CqrsDemo.BuildingBlocks.Domain;

namespace Cart.Domain.Events;

public sealed record CartCreatedEvent(Guid CartId, Guid CustomerId, DateTime CreatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record CartItemAddedEvent(
    Guid CartId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record CartItemRemovedEvent(Guid CartId, Guid ProductId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record CartCheckedOutEvent(
    Guid CartId,
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyList<OrderLine> Lines,
    decimal TotalAmount,
    DateTime CheckedOutAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

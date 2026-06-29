using CqrsDemo.BuildingBlocks.Domain;

namespace CqrsDemo.Domain.Orders.Events;

public sealed record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid CartId,
    IReadOnlyList<OrderLine> Lines,
    decimal TotalAmount,
    DateTime CreatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

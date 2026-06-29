using CqrsDemo.BuildingBlocks.Domain;

namespace Order.Domain.Events;

public sealed record OrderCancelledEvent(
    Guid OrderId,
    Guid CartId,
    string Reason,
    DateTime CancelledAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

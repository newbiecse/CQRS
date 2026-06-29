using CqrsDemo.BuildingBlocks.Domain;

namespace CqrsDemo.Domain.Carts.Events;

public sealed record CartCreatedEvent(Guid CartId, Guid CustomerId, DateTime CreatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

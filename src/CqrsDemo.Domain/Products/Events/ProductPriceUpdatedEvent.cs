using CqrsDemo.BuildingBlocks.Domain;

namespace CqrsDemo.Domain.Products.Events;

public sealed record ProductPriceUpdatedEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice,
    DateTime UpdatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

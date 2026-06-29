using CqrsDemo.BuildingBlocks.Domain;

namespace Product.Domain.Events;

public sealed record ProductCreatedEvent(Guid ProductId, string Name, decimal Price, DateTime CreatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record ProductPriceUpdatedEvent(Guid ProductId, decimal OldPrice, decimal NewPrice, DateTime UpdatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

using CqrsDemo.BuildingBlocks.Domain;

namespace CqrsDemo.Domain.Products.Events;

public sealed record ProductCreatedEvent(
    Guid ProductId,
    string Name,
    decimal Price,
    DateTime CreatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

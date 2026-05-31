using CqrsDemo.BuildingBlocks.Domain;

namespace User.Domain.Events;

public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTime RegisteredAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record UserProfileUpdatedEvent(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTime UpdatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record UserDeactivatedEvent(Guid UserId, DateTime DeactivatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

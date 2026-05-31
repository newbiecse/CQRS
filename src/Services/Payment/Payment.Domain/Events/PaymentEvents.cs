using CqrsDemo.BuildingBlocks.Domain;

namespace Payment.Domain.Events;

public sealed record PaymentInitiatedEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    DateTime InitiatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record PaymentCompletedEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    DateTime CompletedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record PaymentFailedEvent(
    Guid PaymentId,
    Guid OrderId,
    string Reason,
    DateTime FailedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

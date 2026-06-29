using CqrsDemo.BuildingBlocks.Domain;

namespace CqrsDemo.Domain.Payments.Events;

public sealed record PaymentCompletedEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    DateTime CompletedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

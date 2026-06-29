using CqrsDemo.BuildingBlocks.Domain;

namespace CqrsDemo.Domain.Payments.Events;

public sealed record PaymentFailedEvent(
    Guid PaymentId,
    Guid OrderId,
    string Reason,
    DateTime FailedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

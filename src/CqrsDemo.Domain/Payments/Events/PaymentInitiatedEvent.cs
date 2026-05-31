using CqrsDemo.Domain.Common;

namespace CqrsDemo.Domain.Payments.Events;

public sealed record PaymentInitiatedEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    DateTime InitiatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

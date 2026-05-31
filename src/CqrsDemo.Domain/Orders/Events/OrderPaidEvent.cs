using CqrsDemo.Domain.Common;

namespace CqrsDemo.Domain.Orders.Events;

public sealed record OrderPaidEvent(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    DateTime PaidAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

using CqrsDemo.BuildingBlocks.Domain;

namespace Order.Domain.Events;

public sealed record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid CartId,
    IReadOnlyList<OrderLine> Lines,
    decimal TotalAmount,
    DateTime CreatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record OrderPaidEvent(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    DateTime PaidAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record OrderUpdatedEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid CartId,
    IReadOnlyList<OrderLine> Lines,
    decimal TotalAmount,
    DateTime UpdatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

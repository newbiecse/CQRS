using CqrsDemo.BuildingBlocks.Domain;

namespace CqrsDemo.Domain.Carts.Events;

public sealed record CartCheckedOutEvent(
    Guid CartId,
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyList<OrderLine> Lines,
    decimal TotalAmount,
    DateTime CheckedOutAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

using CqrsDemo.Domain.Common;

namespace CqrsDemo.Domain.Carts.Events;

public sealed record CartItemRemovedEvent(Guid CartId, Guid ProductId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

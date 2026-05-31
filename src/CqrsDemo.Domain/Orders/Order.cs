using CqrsDemo.Domain.Common;
using CqrsDemo.Domain.Orders.Events;

namespace CqrsDemo.Domain.Orders;

public class Order : AggregateRoot
{
    public const string StreamType = "Order";

    public Guid CustomerId { get; private set; }
    public Guid CartId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private readonly List<OrderLine> _lines = [];
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public decimal TotalAmount { get; private set; }

    private Order()
    {
    }

    public static Order Create(Guid customerId, Guid cartId, IReadOnlyList<OrderLine> lines)
    {
        if (lines.Count == 0)
        {
            throw new ArgumentException("Order must contain at least one line.", nameof(lines));
        }

        var order = new Order();
        var total = lines.Sum(l => l.LineTotal);
        order.ApplyNew(new OrderCreatedEvent(
            Guid.NewGuid(),
            customerId,
            cartId,
            lines.ToList(),
            total,
            DateTime.UtcNow));

        return order;
    }

    public static Order LoadFromHistory(IReadOnlyList<IDomainEvent> history)
    {
        var order = new Order();
        foreach (var domainEvent in history)
        {
            order.Apply(domainEvent);
        }

        order.SetVersion(history.Count);
        order.ClearDomainEvents();
        return order;
    }

    public void MarkAsPaid(Guid paymentId, decimal amount)
    {
        if (Status == OrderStatus.Paid)
        {
            throw new InvalidOperationException("Order is already paid.");
        }

        if (amount != TotalAmount)
        {
            throw new ArgumentException(
                $"Payment amount {amount} does not match order total {TotalAmount}.",
                nameof(amount));
        }

        ApplyNew(new OrderPaidEvent(Id, paymentId, amount, DateTime.UtcNow));
    }

    private void ApplyNew(IDomainEvent domainEvent)
    {
        Apply(domainEvent);
        RaiseDomainEvent(domainEvent);
    }

    internal void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case OrderCreatedEvent created:
                Id = created.OrderId;
                CustomerId = created.CustomerId;
                CartId = created.CartId;
                _lines.Clear();
                _lines.AddRange(created.Lines);
                TotalAmount = created.TotalAmount;
                CreatedAt = created.CreatedAt;
                Status = OrderStatus.PendingPayment;
                break;

            case OrderPaidEvent:
                Status = OrderStatus.Paid;
                break;

            default:
                throw new NotSupportedException(
                    $"Domain event {domainEvent.GetType().Name} is not supported by {nameof(Order)}.");
        }
    }
}

using CqrsDemo.BuildingBlocks.Domain;
using Order.Domain.Events;

namespace Order.Domain;

public sealed class OrderAggregate : AggregateRoot
{
    public const string StreamType = "Order";

    public Guid CustomerId { get; private set; }
    public Guid CartId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private readonly List<OrderLine> _lines = [];
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public decimal TotalAmount { get; private set; }

    private OrderAggregate() { }

    public static OrderAggregate Create(
        Guid orderId,
        Guid customerId,
        Guid cartId,
        IReadOnlyList<OrderLine> lines)
    {
        if (lines.Count == 0) throw new ArgumentException("Order must contain at least one line.", nameof(lines));
        var order = new OrderAggregate();
        var total = lines.Sum(l => l.LineTotal);
        order.Raise(new OrderCreatedEvent(orderId, customerId, cartId, lines.ToList(), total, DateTime.UtcNow));
        return order;
    }

    public static OrderAggregate Load(IReadOnlyList<IDomainEvent> history)
    {
        var order = new OrderAggregate();
        foreach (var e in history) order.Apply(e);
        order.SetVersion(history.Count);
        order.ClearDomainEvents();
        return order;
    }

    public void MarkAsPaid(Guid paymentId, decimal amount)
    {
        if (Status == OrderStatus.Cancelled) throw new InvalidOperationException("Cannot pay a cancelled order.");
        if (Status == OrderStatus.Paid) throw new InvalidOperationException("Order is already paid.");
        if (amount != TotalAmount)
            throw new ArgumentException($"Payment amount {amount} does not match order total {TotalAmount}.", nameof(amount));
        Raise(new OrderPaidEvent(Id, paymentId, amount, DateTime.UtcNow));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Cancelled) throw new InvalidOperationException("Order is already cancelled.");
        if (Status == OrderStatus.Paid) throw new InvalidOperationException("Cannot cancel a paid order.");
        Raise(new OrderCancelledEvent(Id, CartId, reason, DateTime.UtcNow));
    }

    private void Raise(IDomainEvent e)
    {
        Apply(e);
        RaiseDomainEvent(e);
    }

    private void Apply(IDomainEvent e)
    {
        switch (e)
        {
            case OrderCreatedEvent created:
                ApplyCreated(created);
                break;
            case OrderPaidEvent:
                Status = OrderStatus.Paid;
                break;
            case OrderCancelledEvent:
                Status = OrderStatus.Cancelled;
                break;
            default:
                throw new NotSupportedException(e.GetType().Name);
        }
    }

    private void ApplyCreated(OrderCreatedEvent created)
    {
        Id = created.OrderId;
        CustomerId = created.CustomerId;
        CartId = created.CartId;
        _lines.Clear();
        _lines.AddRange(created.Lines);
        TotalAmount = created.TotalAmount;
        CreatedAt = created.CreatedAt;
        Status = OrderStatus.PendingPayment;
    }
}

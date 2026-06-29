using CqrsDemo.BuildingBlocks.Domain;
using Order.Domain.Events;

namespace Order.Domain;

public sealed class OrderAggregate : AggregateRoot
{
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

        var total = lines.Sum(l => l.LineTotal);
        var order = new OrderAggregate
        {
            Id = orderId,
            CustomerId = customerId,
            CartId = cartId,
            Status = OrderStatus.PendingPayment,
            TotalAmount = total,
            CreatedAt = DateTime.UtcNow
        };
        order._lines.AddRange(lines);
        order.RaiseDomainEvent(new OrderCreatedEvent(orderId, customerId, cartId, lines.ToList(), total, order.CreatedAt));
        return order;
    }

    public static OrderAggregate Restore(
        Guid id,
        Guid customerId,
        Guid cartId,
        OrderStatus status,
        decimal totalAmount,
        DateTime createdAt,
        IReadOnlyList<OrderLine> lines)
    {
        var order = new OrderAggregate
        {
            Id = id,
            CustomerId = customerId,
            CartId = cartId,
            Status = status,
            TotalAmount = totalAmount,
            CreatedAt = createdAt
        };
        order._lines.AddRange(lines);
        return order;
    }

    public void MarkAsPaid(Guid paymentId, decimal amount)
    {
        if (Status == OrderStatus.Cancelled) throw new InvalidOperationException("Cannot pay a cancelled order.");
        if (Status == OrderStatus.Paid) throw new InvalidOperationException("Order is already paid.");
        if (amount != TotalAmount)
            throw new ArgumentException($"Payment amount {amount} does not match order total {TotalAmount}.", nameof(amount));

        Status = OrderStatus.Paid;
        RaiseDomainEvent(new OrderPaidEvent(Id, paymentId, amount, DateTime.UtcNow));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Cancelled) throw new InvalidOperationException("Order is already cancelled.");
        if (Status == OrderStatus.Paid) throw new InvalidOperationException("Cannot cancel a paid order.");

        Status = OrderStatus.Cancelled;
        RaiseDomainEvent(new OrderCancelledEvent(Id, CartId, reason, DateTime.UtcNow));
    }

    public void UpdateLines(IReadOnlyList<OrderLine> lines)
    {
        if (Status != OrderStatus.PendingPayment)
            throw new InvalidOperationException("Only pending orders can be updated.");
        if (lines.Count == 0)
            throw new ArgumentException("Order must contain at least one line.", nameof(lines));

        _lines.Clear();
        _lines.AddRange(lines);
        TotalAmount = lines.Sum(l => l.LineTotal);
        RaiseDomainEvent(new OrderUpdatedEvent(Id, CustomerId, CartId, lines.ToList(), TotalAmount, DateTime.UtcNow));
    }
}

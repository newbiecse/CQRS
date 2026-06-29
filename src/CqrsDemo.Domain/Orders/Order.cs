using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.Domain.Orders.Events;

namespace CqrsDemo.Domain.Orders;

public sealed class Order : AggregateRoot
{
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

        var total = lines.Sum(l => l.LineTotal);
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CartId = cartId,
            Status = OrderStatus.PendingPayment,
            TotalAmount = total,
            CreatedAt = DateTime.UtcNow
        };
        order._lines.AddRange(lines);
        order.RaiseDomainEvent(new OrderCreatedEvent(order.Id, customerId, cartId, lines.ToList(), total, order.CreatedAt));
        return order;
    }

    public static Order Restore(
        Guid id,
        Guid customerId,
        Guid cartId,
        OrderStatus status,
        decimal totalAmount,
        DateTime createdAt,
        IReadOnlyList<OrderLine> lines)
    {
        var order = new Order
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

        Status = OrderStatus.Paid;
        RaiseDomainEvent(new OrderPaidEvent(Id, paymentId, amount, DateTime.UtcNow));
    }
}

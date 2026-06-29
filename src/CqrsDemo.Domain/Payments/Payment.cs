using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.Domain.Payments.Events;

namespace CqrsDemo.Domain.Payments;

public sealed class Payment : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime InitiatedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private Payment()
    {
    }

    public static Payment Initiate(Guid orderId, decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = amount,
            Status = PaymentStatus.Pending,
            InitiatedAt = DateTime.UtcNow
        };
        payment.RaiseDomainEvent(new PaymentInitiatedEvent(payment.Id, orderId, amount, payment.InitiatedAt));
        return payment;
    }

    public static Payment Restore(
        Guid id,
        Guid orderId,
        decimal amount,
        PaymentStatus status,
        DateTime initiatedAt,
        string? failureReason) =>
        new()
        {
            Id = id,
            OrderId = orderId,
            Amount = amount,
            Status = status,
            InitiatedAt = initiatedAt,
            FailureReason = failureReason
        };

    public void Complete()
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("Only pending payments can be completed.");
        }

        Status = PaymentStatus.Completed;
        FailureReason = null;
        RaiseDomainEvent(new PaymentCompletedEvent(Id, OrderId, Amount, DateTime.UtcNow));
    }

    public void Fail(string reason)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("Only pending payments can fail.");
        }

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        RaiseDomainEvent(new PaymentFailedEvent(Id, OrderId, reason, DateTime.UtcNow));
    }
}

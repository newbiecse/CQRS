using CqrsDemo.BuildingBlocks.Domain;
using Payment.Domain.Events;

namespace Payment.Domain;

public sealed class PaymentAggregate : AggregateRoot
{
    public const string StreamType = "Payment";

    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime InitiatedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private PaymentAggregate() { }

    public static PaymentAggregate Initiate(Guid orderId, decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        var payment = new PaymentAggregate();
        payment.Raise(new PaymentInitiatedEvent(Guid.NewGuid(), orderId, amount, DateTime.UtcNow));
        return payment;
    }

    public static PaymentAggregate Load(IReadOnlyList<IDomainEvent> history)
    {
        var payment = new PaymentAggregate();
        foreach (var e in history) payment.Apply(e);
        payment.SetVersion(history.Count);
        payment.ClearDomainEvents();
        return payment;
    }

    public void Complete()
    {
        if (Status != PaymentStatus.Pending) throw new InvalidOperationException("Only pending payments can be completed.");
        Raise(new PaymentCompletedEvent(Id, OrderId, Amount, DateTime.UtcNow));
    }

    public void Fail(string reason)
    {
        if (Status != PaymentStatus.Pending) throw new InvalidOperationException("Only pending payments can fail.");
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Failure reason is required.", nameof(reason));
        Raise(new PaymentFailedEvent(Id, OrderId, reason, DateTime.UtcNow));
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
            case PaymentInitiatedEvent initiated:
                ApplyInitiated(initiated);
                break;
            case PaymentCompletedEvent:
                Status = PaymentStatus.Completed;
                FailureReason = null;
                break;
            case PaymentFailedEvent failed:
                Status = PaymentStatus.Failed;
                FailureReason = failed.Reason;
                break;
            default:
                throw new NotSupportedException(e.GetType().Name);
        }
    }

    private void ApplyInitiated(PaymentInitiatedEvent initiated)
    {
        Id = initiated.PaymentId;
        OrderId = initiated.OrderId;
        Amount = initiated.Amount;
        InitiatedAt = initiated.InitiatedAt;
        Status = PaymentStatus.Pending;
    }
}

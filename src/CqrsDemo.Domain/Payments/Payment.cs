using CqrsDemo.Domain.Common;
using CqrsDemo.Domain.Payments.Events;

namespace CqrsDemo.Domain.Payments;

public class Payment : AggregateRoot
{
    public const string StreamType = "Payment";

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

        var payment = new Payment();
        payment.ApplyNew(new PaymentInitiatedEvent(Guid.NewGuid(), orderId, amount, DateTime.UtcNow));
        return payment;
    }

    public static Payment LoadFromHistory(IReadOnlyList<IDomainEvent> history)
    {
        var payment = new Payment();
        foreach (var domainEvent in history)
        {
            payment.Apply(domainEvent);
        }

        payment.SetVersion(history.Count);
        payment.ClearDomainEvents();
        return payment;
    }

    public void Complete()
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("Only pending payments can be completed.");
        }

        ApplyNew(new PaymentCompletedEvent(Id, OrderId, Amount, DateTime.UtcNow));
    }

    public void Fail(string reason)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("Only pending payments can fail.");
        }

        ApplyNew(new PaymentFailedEvent(Id, OrderId, reason, DateTime.UtcNow));
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
            case PaymentInitiatedEvent initiated:
                Id = initiated.PaymentId;
                OrderId = initiated.OrderId;
                Amount = initiated.Amount;
                InitiatedAt = initiated.InitiatedAt;
                Status = PaymentStatus.Pending;
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
                throw new NotSupportedException(
                    $"Domain event {domainEvent.GetType().Name} is not supported by {nameof(Payment)}.");
        }
    }
}

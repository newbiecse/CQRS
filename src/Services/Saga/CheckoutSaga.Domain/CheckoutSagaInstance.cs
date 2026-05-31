using CqrsDemo.Contracts.Saga;

namespace CheckoutSaga.Domain;

public sealed class CheckoutSagaInstance
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? PaymentId { get; set; }
    public bool SimulatePaymentFailure { get; set; }
    public string State { get; set; } = CheckoutSagaStates.Started;
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool IsTerminal =>
        State is CheckoutSagaStates.Completed
            or CheckoutSagaStates.Compensated
            or CheckoutSagaStates.Failed;
}

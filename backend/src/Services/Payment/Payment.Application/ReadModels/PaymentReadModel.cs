namespace Payment.Application.ReadModels;

public sealed class PaymentReadModel
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? FailureReason { get; init; }
    public DateTime InitiatedAt { get; init; }
    public DateTime LastUpdatedAt { get; init; }
}

namespace CqrsDemo.Queries.Infrastructure.Persistence.Read;

public sealed class PaymentReadEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public DateTime InitiatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}

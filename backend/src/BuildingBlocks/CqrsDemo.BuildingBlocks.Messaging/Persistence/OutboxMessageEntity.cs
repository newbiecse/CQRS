namespace CqrsDemo.BuildingBlocks.Messaging.Persistence;

public sealed class OutboxMessageEntity
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public string? TraceId { get; set; }
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
}

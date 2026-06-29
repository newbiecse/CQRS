namespace Audit.Application.Models;

public sealed class BusinessAuditRecord
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public DateTime OccurredAtUtc { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Service { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string PayloadJson { get; init; } = string.Empty;
}

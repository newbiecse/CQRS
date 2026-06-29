namespace Audit.Application.Abstractions;

public sealed class BusinessAuditSearchQuery
{
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public string? EventType { get; init; }
    public string? SearchText { get; init; }
    public int Size { get; init; } = 50;
}

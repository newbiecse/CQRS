namespace CqrsDemo.Commands.Infrastructure.Persistence.EventStore;

public sealed class StoredEventEntity
{
    public Guid Id { get; set; }
    public Guid StreamId { get; set; }
    public string StreamType { get; set; } = string.Empty;
    public long Version { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
}

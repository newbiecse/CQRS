using CqrsDemo.BuildingBlocks.Domain;

namespace CqrsDemo.BuildingBlocks.EventStore.Abstractions;

public sealed record StoredEventDto(
    Guid Id, Guid StreamId, string StreamType, long Version,
    string EventType, string Payload, DateTime OccurredOn);

public interface IEventStore
{
    Task<TAggregate?> LoadAsync<TAggregate>(Guid streamId, string streamType,
        Func<IReadOnlyList<IDomainEvent>, TAggregate> factory, CancellationToken ct = default)
        where TAggregate : AggregateRoot;

    Task SaveNewAsync(AggregateRoot aggregate, string streamType, CancellationToken ct = default);
    Task SaveAsync(AggregateRoot aggregate, string streamType, CancellationToken ct = default);
    Task<IReadOnlyList<StoredEventDto>> GetHistoryAsync(Guid streamId, string streamType, CancellationToken ct = default);
}

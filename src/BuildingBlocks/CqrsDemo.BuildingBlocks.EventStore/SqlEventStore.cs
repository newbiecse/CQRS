using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using CqrsDemo.BuildingBlocks.EventStore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CqrsDemo.BuildingBlocks.EventStore;

public sealed class SqlEventStore(
    EventStoreDbContext dbContext,
    IDomainEventSerializer serializer,
    IIntegrationEventMapper mapper) : IEventStore
{
    public async Task<TAggregate?> LoadAsync<TAggregate>(Guid streamId, string streamType,
        Func<IReadOnlyList<IDomainEvent>, TAggregate> factory, CancellationToken ct = default)
        where TAggregate : AggregateRoot
    {
        var stored = await dbContext.StoredEvents.AsNoTracking()
            .Where(e => e.StreamId == streamId && e.StreamType == streamType)
            .OrderBy(e => e.Version).ToListAsync(ct);
        if (stored.Count == 0) return null;
        return factory(stored.Select(e => serializer.Deserialize(e.EventType, e.Payload)).ToList());
    }

    public Task SaveNewAsync(AggregateRoot aggregate, string streamType, CancellationToken ct = default) =>
        AppendAsync(aggregate, streamType, 0, ct);

    public Task SaveAsync(AggregateRoot aggregate, string streamType, CancellationToken ct = default) =>
        AppendAsync(aggregate, streamType, aggregate.Version, ct);

    public async Task<IReadOnlyList<StoredEventDto>> GetHistoryAsync(Guid streamId, string streamType, CancellationToken ct = default)
    {
        var stored = await dbContext.StoredEvents.AsNoTracking()
            .Where(e => e.StreamId == streamId && e.StreamType == streamType)
            .OrderBy(e => e.Version).ToListAsync(ct);
        return stored.Select(e => new StoredEventDto(e.Id, e.StreamId, e.StreamType, e.Version, e.EventType, e.Payload, e.OccurredOn)).ToList();
    }

    private async Task AppendAsync(AggregateRoot aggregate, string streamType, long expectedVersion, CancellationToken ct)
    {
        var pending = aggregate.DomainEvents.ToList();
        if (pending.Count == 0) return;

        var current = await dbContext.StoredEvents
            .Where(e => e.StreamId == aggregate.Id && e.StreamType == streamType)
            .MaxAsync(e => (long?)e.Version, ct) ?? 0;
        if (current != expectedVersion) throw new ConcurrencyException(aggregate.Id, expectedVersion, current);

        var version = current;
        foreach (var ev in pending)
        {
            version++;
            dbContext.StoredEvents.Add(new StoredEventEntity
            {
                Id = Guid.NewGuid(), StreamId = aggregate.Id, StreamType = streamType, Version = version,
                EventType = serializer.GetTypeName(ev), Payload = serializer.Serialize(ev), OccurredOn = ev.OccurredOn
            });
        }

        foreach (var msg in mapper.Map(pending))
        {
            dbContext.OutboxMessages.Add(new OutboxMessageEntity
            {
                Id = Guid.NewGuid(), EventType = msg.EventType, Payload = msg.Payload,
                OccurredOn = DateTime.UtcNow, AttemptCount = 0
            });
        }

        aggregate.SetVersion(version);
        aggregate.ClearDomainEvents();
        await dbContext.SaveChangesAsync(ct);
    }
}

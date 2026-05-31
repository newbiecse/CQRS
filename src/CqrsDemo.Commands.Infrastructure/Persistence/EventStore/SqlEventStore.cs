using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Commands.Application.Integration;
using CqrsDemo.Commands.Infrastructure.Persistence.Outbox;
using CqrsDemo.Commands.Infrastructure.Persistence.Write;
using CqrsDemo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CqrsDemo.Commands.Infrastructure.Persistence.EventStore;

public sealed class SqlEventStore(WriteDbContext dbContext) : IEventStore
{
    public async Task<TAggregate?> LoadAsync<TAggregate>(
        Guid streamId,
        string streamType,
        Func<IReadOnlyList<IDomainEvent>, TAggregate> factory,
        CancellationToken cancellationToken = default)
        where TAggregate : AggregateRoot
    {
        var storedEvents = await dbContext.StoredEvents
            .AsNoTracking()
            .Where(e => e.StreamId == streamId && e.StreamType == streamType)
            .OrderBy(e => e.Version)
            .ToListAsync(cancellationToken);

        if (storedEvents.Count == 0)
        {
            return null;
        }

        var domainEvents = storedEvents
            .Select(e => DomainEventSerializer.Deserialize(e.EventType, e.Payload))
            .ToList();

        return factory(domainEvents);
    }

    public Task SaveNewAsync(
        AggregateRoot aggregate,
        string streamType,
        CancellationToken cancellationToken = default) =>
        AppendEventsAsync(aggregate, streamType, expectedVersion: 0, cancellationToken);

    public Task SaveAsync(
        AggregateRoot aggregate,
        string streamType,
        CancellationToken cancellationToken = default) =>
        AppendEventsAsync(aggregate, streamType, aggregate.Version, cancellationToken);

    public async Task<IReadOnlyList<StoredEventDto>> GetHistoryAsync(
        Guid streamId,
        string streamType,
        CancellationToken cancellationToken = default)
    {
        var storedEvents = await dbContext.StoredEvents
            .AsNoTracking()
            .Where(e => e.StreamId == streamId && e.StreamType == streamType)
            .OrderBy(e => e.Version)
            .ToListAsync(cancellationToken);

        return storedEvents
            .Select(e => new StoredEventDto(
                e.Id,
                e.StreamId,
                e.StreamType,
                e.Version,
                e.EventType,
                e.Payload,
                e.OccurredOn))
            .ToList();
    }

    private async Task AppendEventsAsync(
        AggregateRoot aggregate,
        string streamType,
        long expectedVersion,
        CancellationToken cancellationToken)
    {
        var pendingEvents = aggregate.DomainEvents.ToList();
        if (pendingEvents.Count == 0)
        {
            return;
        }

        var currentVersion = await dbContext.StoredEvents
            .Where(e => e.StreamId == aggregate.Id && e.StreamType == streamType)
            .MaxAsync(e => (long?)e.Version, cancellationToken) ?? 0;

        if (currentVersion != expectedVersion)
        {
            throw new ConcurrencyException(aggregate.Id, expectedVersion, currentVersion);
        }

        var nextVersion = currentVersion;

        foreach (var domainEvent in pendingEvents)
        {
            nextVersion++;

            dbContext.StoredEvents.Add(new StoredEventEntity
            {
                Id = Guid.NewGuid(),
                StreamId = aggregate.Id,
                StreamType = streamType,
                Version = nextVersion,
                EventType = DomainEventSerializer.GetTypeName(domainEvent),
                Payload = DomainEventSerializer.Serialize(domainEvent),
                OccurredOn = domainEvent.OccurredOn
            });
        }

        AddOutboxMessages(pendingEvents);
        aggregate.SetVersion(nextVersion);
        aggregate.ClearDomainEvents();

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void AddOutboxMessages(IReadOnlyList<IDomainEvent> domainEvents)
    {
        var outboxDtos = DomainEventToOutboxMapper.Map(domainEvents);

        foreach (var message in outboxDtos)
        {
            dbContext.OutboxMessages.Add(new OutboxMessageEntity
            {
                Id = Guid.NewGuid(),
                EventType = message.EventType,
                Payload = message.Payload,
                OccurredOn = DateTime.UtcNow,
                AttemptCount = 0
            });
        }
    }
}

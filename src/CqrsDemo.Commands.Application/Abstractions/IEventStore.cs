using CqrsDemo.Domain.Common;

namespace CqrsDemo.Commands.Application.Abstractions;

public interface IEventStore
{
    Task<TAggregate?> LoadAsync<TAggregate>(
        Guid streamId,
        string streamType,
        Func<IReadOnlyList<IDomainEvent>, TAggregate> factory,
        CancellationToken cancellationToken = default)
        where TAggregate : AggregateRoot;

    Task SaveNewAsync(
        AggregateRoot aggregate,
        string streamType,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        AggregateRoot aggregate,
        string streamType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredEventDto>> GetHistoryAsync(
        Guid streamId,
        string streamType,
        CancellationToken cancellationToken = default);
}

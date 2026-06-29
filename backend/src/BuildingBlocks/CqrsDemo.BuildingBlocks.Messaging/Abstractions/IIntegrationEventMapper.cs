using CqrsDemo.BuildingBlocks.Domain;

namespace CqrsDemo.BuildingBlocks.Messaging.Abstractions;

public sealed record OutboxMessageDto(string EventType, string Payload);

public interface IIntegrationEventMapper
{
    IReadOnlyList<OutboxMessageDto> Map(IEnumerable<IDomainEvent> domainEvents);
}

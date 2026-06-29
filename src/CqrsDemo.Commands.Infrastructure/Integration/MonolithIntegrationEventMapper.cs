using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.Commands.Application.Integration;

namespace CqrsDemo.Commands.Infrastructure.Integration;

public sealed class MonolithIntegrationEventMapper : IIntegrationEventMapper
{
    public IReadOnlyList<OutboxMessageDto> Map(IEnumerable<IDomainEvent> domainEvents) =>
        DomainEventToOutboxMapper.Map(domainEvents);
}

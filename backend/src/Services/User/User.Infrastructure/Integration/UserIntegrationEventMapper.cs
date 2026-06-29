using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Serialization;
using CqrsDemo.Contracts.Users;
using User.Domain.Events;

namespace User.Infrastructure.Integration;

public sealed class UserIntegrationEventMapper : IIntegrationEventMapper
{
    public IReadOnlyList<OutboxMessageDto> Map(IEnumerable<IDomainEvent> domainEvents) =>
        domainEvents.Select(MapSingle).ToList();

    private static OutboxMessageDto MapSingle(IDomainEvent domainEvent) => domainEvent switch
    {
        UserRegisteredEvent e => new(
            IntegrationEventTypes.UserRegistered,
            IntegrationEventSerializer.Serialize(new UserRegisteredIntegrationEvent(
                e.UserId, e.Email, e.DisplayName, e.RegisteredAt))),
        UserProfileUpdatedEvent e => new(
            IntegrationEventTypes.UserProfileUpdated,
            IntegrationEventSerializer.Serialize(new UserProfileUpdatedIntegrationEvent(
                e.UserId, e.Email, e.DisplayName, e.UpdatedAt))),
        UserDeactivatedEvent e => new(
            IntegrationEventTypes.UserDeactivated,
            IntegrationEventSerializer.Serialize(new UserDeactivatedIntegrationEvent(
                e.UserId, e.DeactivatedAt))),
        _ => throw new NotSupportedException($"Domain event {domainEvent.GetType().Name} is not mapped.")
    };
}

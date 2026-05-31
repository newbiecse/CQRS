using CqrsDemo.BuildingBlocks.Domain;

namespace CqrsDemo.BuildingBlocks.EventStore.Abstractions;

public interface IDomainEventSerializer
{
    string GetTypeName(IDomainEvent domainEvent);
    string Serialize(IDomainEvent domainEvent);
    IDomainEvent Deserialize(string eventType, string payload);
}

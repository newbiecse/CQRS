using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Products;
using CqrsDemo.Contracts.Serialization;
using Product.Domain.Events;

namespace Product.Infrastructure.EventStore;

public sealed class ProductIntegrationEventMapper : IIntegrationEventMapper
{
    public IReadOnlyList<OutboxMessageDto> Map(IEnumerable<IDomainEvent> domainEvents) =>
        domainEvents.Select(MapSingle).ToList();

    private static OutboxMessageDto MapSingle(IDomainEvent e) => e switch
    {
        ProductCreatedEvent c => new(IntegrationEventTypes.ProductCreated,
            IntegrationEventSerializer.Serialize(new ProductCreatedIntegrationEvent(c.ProductId, c.Name, c.Price, c.CreatedAt))),
        ProductPriceUpdatedEvent u => new(IntegrationEventTypes.ProductPriceUpdated,
            IntegrationEventSerializer.Serialize(new ProductPriceUpdatedIntegrationEvent(u.ProductId, u.OldPrice, u.NewPrice, u.UpdatedAt))),
        _ => throw new NotSupportedException(e.GetType().Name)
    };
}

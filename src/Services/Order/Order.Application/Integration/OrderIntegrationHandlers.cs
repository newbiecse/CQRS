using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.Contracts.Carts;
using CqrsDemo.Contracts.Serialization;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using Order.Domain;

namespace Order.Application.Integration;

public sealed class OrderIntegrationHandlers(IEventStore eventStore)
{
    public async Task HandleCartCheckedOutAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<CartCheckedOutIntegrationEvent>(payload);
        var existing = await eventStore.LoadAsync(
            e.OrderId,
            OrderAggregate.StreamType,
            OrderAggregate.Load,
            cancellationToken);
        if (existing is not null) return;

        var lines = e.Lines.Select(l => new OrderLine
        {
            ProductId = l.ProductId,
            ProductName = l.ProductName,
            UnitPrice = l.UnitPrice,
            Quantity = l.Quantity
        }).ToList();

        var order = OrderAggregate.Create(e.OrderId, e.CustomerId, e.CartId, lines);
        await eventStore.SaveNewAsync(order, OrderAggregate.StreamType, cancellationToken);
    }
}

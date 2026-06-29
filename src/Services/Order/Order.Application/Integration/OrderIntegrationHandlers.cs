using CqrsDemo.Contracts.Carts;
using CqrsDemo.Contracts.Serialization;
using CqrsDemo.BuildingBlocks.Domain;
using Order.Application.Abstractions;
using Order.Domain;

namespace Order.Application.Integration;

public sealed class OrderIntegrationHandlers(IOrderWriteRepository repository)
{
    public async Task HandleCartCheckedOutAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<CartCheckedOutIntegrationEvent>(payload);
        if (await repository.ExistsAsync(e.OrderId, cancellationToken)) return;

        var lines = e.Lines.Select(l => new OrderLine
        {
            ProductId = l.ProductId,
            ProductName = l.ProductName,
            UnitPrice = l.UnitPrice,
            Quantity = l.Quantity
        }).ToList();

        var order = OrderAggregate.Create(e.OrderId, e.CustomerId, e.CartId, lines);
        await repository.AddAsync(order, cancellationToken);
    }
}

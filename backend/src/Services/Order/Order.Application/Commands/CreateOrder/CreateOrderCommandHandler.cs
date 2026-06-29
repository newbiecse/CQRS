using CqrsDemo.BuildingBlocks.Domain;
using MediatR;
using Order.Application.Abstractions;
using Order.Domain;

namespace Order.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(
    IOrderWriteRepository repository,
    IInventoryCommandClient inventoryClient)
    : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Lines.Count == 0)
            throw new ArgumentException("At least one line is required.", nameof(request.Lines));

        var orderId = request.OrderId ?? Guid.NewGuid();
        if (await repository.ExistsAsync(orderId, cancellationToken))
            throw new InvalidOperationException($"Order {orderId} already exists.");

        var lines = request.Lines.Select(l => new OrderLine
        {
            ProductId = l.ProductId,
            ProductName = l.ProductName,
            UnitPrice = l.UnitPrice,
            Quantity = l.Quantity
        }).ToList();

        var stockLines = lines.Select(l => new InventoryStockLine(l.ProductId, l.Quantity)).ToList();
        await inventoryClient.ReserveAsync(orderId, stockLines, cancellationToken);

        try
        {
            var order = OrderAggregate.Create(orderId, request.CustomerId, request.CartId ?? Guid.Empty, lines);
            await repository.AddAsync(order, cancellationToken);
            return orderId;
        }
        catch
        {
            await inventoryClient.ReleaseAsync(orderId, cancellationToken);
            throw;
        }
    }
}

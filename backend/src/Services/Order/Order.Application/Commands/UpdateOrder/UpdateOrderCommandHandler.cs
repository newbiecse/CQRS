using CqrsDemo.BuildingBlocks.Domain;
using MediatR;
using Order.Application.Abstractions;
using Order.Domain;

namespace Order.Application.Commands.UpdateOrder;

public sealed class UpdateOrderCommandHandler(
    IOrderWriteRepository repository,
    IInventoryCommandClient inventoryClient)
    : IRequestHandler<UpdateOrderCommand>
{
    public async Task Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Lines.Count == 0)
            throw new ArgumentException("At least one line is required.", nameof(request.Lines));

        var order = await repository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        var previousLines = order.Lines
            .Select(l => new InventoryStockLine(l.ProductId, l.Quantity))
            .ToList();

        await inventoryClient.ReleaseAsync(request.OrderId, cancellationToken);

        var newLines = request.Lines.Select(l => new OrderLine
        {
            ProductId = l.ProductId,
            ProductName = l.ProductName,
            UnitPrice = l.UnitPrice,
            Quantity = l.Quantity
        }).ToList();

        var newStockLines = newLines.Select(l => new InventoryStockLine(l.ProductId, l.Quantity)).ToList();

        try
        {
            await inventoryClient.ReserveAsync(request.OrderId, newStockLines, cancellationToken);
        }
        catch
        {
            if (previousLines.Count > 0)
                await inventoryClient.ReserveAsync(request.OrderId, previousLines, cancellationToken);
            throw;
        }

        try
        {
            order.UpdateLines(newLines);
            await repository.UpdateAsync(order, cancellationToken);
        }
        catch
        {
            await inventoryClient.ReleaseAsync(request.OrderId, cancellationToken);
            if (previousLines.Count > 0)
                await inventoryClient.ReserveAsync(request.OrderId, previousLines, cancellationToken);
            throw;
        }
    }
}

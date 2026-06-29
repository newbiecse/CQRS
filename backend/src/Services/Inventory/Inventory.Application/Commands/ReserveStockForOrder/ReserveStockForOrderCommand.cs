using Inventory.Application.Abstractions;
using MediatR;

namespace Inventory.Application.Commands.ReserveStockForOrder;

public sealed record ReserveStockForOrderCommand(Guid OrderId, IReadOnlyList<StockLineRequest> Lines) : IRequest;

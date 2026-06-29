using MediatR;

namespace Inventory.Application.Commands.ReleaseStockForOrder;

public sealed record ReleaseStockForOrderCommand(Guid OrderId) : IRequest;

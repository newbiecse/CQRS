using MediatR;

namespace Inventory.Application.Commands.ConfirmStockForOrder;

public sealed record ConfirmStockForOrderCommand(Guid OrderId) : IRequest;

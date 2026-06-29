using MediatR;

namespace Order.Application.Commands.UpdateOrder;

public sealed record UpdateOrderLineInput(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

public sealed record UpdateOrderCommand(Guid OrderId, IReadOnlyList<UpdateOrderLineInput> Lines) : IRequest;

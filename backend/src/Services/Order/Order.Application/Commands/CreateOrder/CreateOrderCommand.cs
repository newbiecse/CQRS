using CqrsDemo.BuildingBlocks.Domain;
using MediatR;

namespace Order.Application.Commands.CreateOrder;

public sealed record OrderLineInput(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

public sealed record CreateOrderCommand(
    Guid CustomerId,
    IReadOnlyList<OrderLineInput> Lines,
    Guid? CartId = null,
    Guid? OrderId = null) : IRequest<Guid>;

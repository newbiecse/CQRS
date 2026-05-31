using MediatR;

namespace CqrsDemo.Commands.Application.Carts.Commands.AddCartItem;

public sealed record AddCartItemCommand(
    Guid CartId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity) : IRequest;

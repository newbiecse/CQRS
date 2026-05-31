using MediatR;

namespace CqrsDemo.Commands.Application.Carts.Commands.RemoveCartItem;

public sealed record RemoveCartItemCommand(Guid CartId, Guid ProductId) : IRequest;

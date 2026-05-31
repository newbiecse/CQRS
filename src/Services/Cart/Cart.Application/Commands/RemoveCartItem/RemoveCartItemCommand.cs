using MediatR;

namespace Cart.Application.Commands.RemoveCartItem;

public sealed record RemoveCartItemCommand(Guid CartId, Guid ProductId) : IRequest;

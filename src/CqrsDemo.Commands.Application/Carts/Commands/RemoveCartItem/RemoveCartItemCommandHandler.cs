using CqrsDemo.Commands.Application.Abstractions;
using MediatR;

namespace CqrsDemo.Commands.Application.Carts.Commands.RemoveCartItem;

public sealed class RemoveCartItemCommandHandler(ICartWriteRepository repository)
    : IRequestHandler<RemoveCartItemCommand>
{
    public async Task Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await repository.GetByIdAsync(request.CartId, cancellationToken)
            ?? throw new KeyNotFoundException($"Cart {request.CartId} was not found.");

        cart.RemoveItem(request.ProductId);
        await repository.UpdateAsync(cart, cancellationToken);
    }
}

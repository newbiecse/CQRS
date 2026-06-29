using Cart.Application.Abstractions;
using Cart.Domain;
using MediatR;

namespace Cart.Application.Commands.AddCartItem;

public sealed class AddCartItemCommandHandler(ICartWriteRepository repository) : IRequestHandler<AddCartItemCommand>
{
    public async Task Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await repository.GetByIdAsync(request.CartId, cancellationToken)
            ?? throw new KeyNotFoundException($"Cart {request.CartId} was not found.");

        cart.AddItem(request.ProductId, request.ProductName, request.UnitPrice, request.Quantity);
        await repository.UpdateAsync(cart, cancellationToken);
    }
}

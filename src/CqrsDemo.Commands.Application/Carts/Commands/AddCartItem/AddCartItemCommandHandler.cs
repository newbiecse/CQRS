using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Domain.Carts;
using MediatR;

namespace CqrsDemo.Commands.Application.Carts.Commands.AddCartItem;

public sealed class AddCartItemCommandHandler(IEventStore eventStore)
    : IRequestHandler<AddCartItemCommand>
{
    public async Task Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await eventStore.LoadAsync(
            request.CartId,
            Cart.StreamType,
            Cart.LoadFromHistory,
            cancellationToken) ?? throw new KeyNotFoundException($"Cart {request.CartId} was not found.");

        cart.AddItem(request.ProductId, request.ProductName, request.UnitPrice, request.Quantity);
        await eventStore.SaveAsync(cart, Cart.StreamType, cancellationToken);
    }
}

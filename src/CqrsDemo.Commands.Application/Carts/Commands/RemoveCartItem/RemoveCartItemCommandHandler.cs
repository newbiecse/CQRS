using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Domain.Carts;
using MediatR;

namespace CqrsDemo.Commands.Application.Carts.Commands.RemoveCartItem;

public sealed class RemoveCartItemCommandHandler(IEventStore eventStore)
    : IRequestHandler<RemoveCartItemCommand>
{
    public async Task Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await eventStore.LoadAsync(
            request.CartId,
            Cart.StreamType,
            Cart.LoadFromHistory,
            cancellationToken) ?? throw new KeyNotFoundException($"Cart {request.CartId} was not found.");

        cart.RemoveItem(request.ProductId);
        await eventStore.SaveAsync(cart, Cart.StreamType, cancellationToken);
    }
}

using Cart.Domain;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using MediatR;

namespace Cart.Application.Commands.RemoveCartItem;

public sealed class RemoveCartItemCommandHandler(IEventStore eventStore) : IRequestHandler<RemoveCartItemCommand>
{
    public async Task Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await eventStore.LoadAsync(
            request.CartId,
            CartAggregate.StreamType,
            CartAggregate.Load,
            cancellationToken) ?? throw new KeyNotFoundException($"Cart {request.CartId} was not found.");

        cart.RemoveItem(request.ProductId);
        await eventStore.SaveAsync(cart, CartAggregate.StreamType, cancellationToken);
    }
}

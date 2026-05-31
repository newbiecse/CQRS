using Cart.Domain;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using MediatR;

namespace Cart.Application.Commands.AddCartItem;

public sealed class AddCartItemCommandHandler(IEventStore eventStore) : IRequestHandler<AddCartItemCommand>
{
    public async Task Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await eventStore.LoadAsync(
            request.CartId,
            CartAggregate.StreamType,
            CartAggregate.Load,
            cancellationToken) ?? throw new KeyNotFoundException($"Cart {request.CartId} was not found.");

        cart.AddItem(request.ProductId, request.ProductName, request.UnitPrice, request.Quantity);
        await eventStore.SaveAsync(cart, CartAggregate.StreamType, cancellationToken);
    }
}

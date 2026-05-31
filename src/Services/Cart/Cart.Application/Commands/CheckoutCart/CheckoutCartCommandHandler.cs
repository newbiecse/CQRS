using Cart.Domain;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using MediatR;

namespace Cart.Application.Commands.CheckoutCart;

public sealed class CheckoutCartCommandHandler(IEventStore eventStore)
    : IRequestHandler<CheckoutCartCommand, Guid>
{
    public async Task<Guid> Handle(CheckoutCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await eventStore.LoadAsync(
            request.CartId,
            CartAggregate.StreamType,
            CartAggregate.Load,
            cancellationToken) ?? throw new KeyNotFoundException($"Cart {request.CartId} was not found.");

        var orderId = Guid.NewGuid();
        cart.Checkout(orderId);
        await eventStore.SaveAsync(cart, CartAggregate.StreamType, cancellationToken);
        return orderId;
    }
}

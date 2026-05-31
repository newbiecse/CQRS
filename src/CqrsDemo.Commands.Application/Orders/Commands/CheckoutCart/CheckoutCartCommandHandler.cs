using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Domain.Carts;
using CqrsDemo.Domain.Orders;
using MediatR;

namespace CqrsDemo.Commands.Application.Orders.Commands.CheckoutCart;

public sealed class CheckoutCartCommandHandler(IEventStore eventStore)
    : IRequestHandler<CheckoutCartCommand, Guid>
{
    public async Task<Guid> Handle(CheckoutCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await eventStore.LoadAsync(
            request.CartId,
            Cart.StreamType,
            Cart.LoadFromHistory,
            cancellationToken) ?? throw new KeyNotFoundException($"Cart {request.CartId} was not found.");

        var order = Order.Create(cart.CustomerId, cart.Id, cart.Items.ToList());
        await eventStore.SaveNewAsync(order, Order.StreamType, cancellationToken);

        cart.Checkout(order.Id);
        await eventStore.SaveAsync(cart, Cart.StreamType, cancellationToken);

        return order.Id;
    }
}

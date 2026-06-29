using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Domain.Carts;
using CqrsDemo.Domain.Orders;
using MediatR;

namespace CqrsDemo.Commands.Application.Orders.Commands.CheckoutCart;

public sealed class CheckoutCartCommandHandler(
    ICartWriteRepository cartRepository,
    IOrderWriteRepository orderRepository)
    : IRequestHandler<CheckoutCartCommand, Guid>
{
    public async Task<Guid> Handle(CheckoutCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByIdAsync(request.CartId, cancellationToken)
            ?? throw new KeyNotFoundException($"Cart {request.CartId} was not found.");

        var order = Order.Create(cart.CustomerId, cart.Id, cart.Items.ToList());
        await orderRepository.AddAsync(order, cancellationToken);

        cart.Checkout(order.Id);
        await cartRepository.UpdateAsync(cart, cancellationToken);

        return order.Id;
    }
}

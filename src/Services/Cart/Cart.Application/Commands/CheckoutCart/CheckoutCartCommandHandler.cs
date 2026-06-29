using Cart.Application.Abstractions;
using Cart.Domain;
using MediatR;

namespace Cart.Application.Commands.CheckoutCart;

public sealed class CheckoutCartCommandHandler(ICartWriteRepository repository)
    : IRequestHandler<CheckoutCartCommand, Guid>
{
    public async Task<Guid> Handle(CheckoutCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await repository.GetByIdAsync(request.CartId, cancellationToken)
            ?? throw new KeyNotFoundException($"Cart {request.CartId} was not found.");

        var orderId = Guid.NewGuid();
        cart.Checkout(orderId);
        await repository.UpdateAsync(cart, cancellationToken);
        return orderId;
    }
}

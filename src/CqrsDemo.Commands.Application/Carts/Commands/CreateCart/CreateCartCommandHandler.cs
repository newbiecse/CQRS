using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Domain.Carts;
using MediatR;

namespace CqrsDemo.Commands.Application.Carts.Commands.CreateCart;

public sealed class CreateCartCommandHandler(IEventStore eventStore)
    : IRequestHandler<CreateCartCommand, Guid>
{
    public async Task<Guid> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var cart = Cart.Create(request.CustomerId);
        await eventStore.SaveNewAsync(cart, Cart.StreamType, cancellationToken);
        return cart.Id;
    }
}

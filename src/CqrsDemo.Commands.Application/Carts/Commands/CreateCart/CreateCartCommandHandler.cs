using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Domain.Carts;
using MediatR;

namespace CqrsDemo.Commands.Application.Carts.Commands.CreateCart;

public sealed class CreateCartCommandHandler(ICartWriteRepository repository)
    : IRequestHandler<CreateCartCommand, Guid>
{
    public async Task<Guid> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var cart = Cart.Create(request.CustomerId);
        await repository.AddAsync(cart, cancellationToken);
        return cart.Id;
    }
}

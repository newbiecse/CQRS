using Cart.Application.Abstractions;
using Cart.Domain;
using MediatR;

namespace Cart.Application.Commands.CreateCart;

public sealed class CreateCartCommandHandler(ICartWriteRepository repository)
    : IRequestHandler<CreateCartCommand, Guid>
{
    public async Task<Guid> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var cart = CartAggregate.Create(request.CustomerId);
        await repository.AddAsync(cart, cancellationToken);
        return cart.Id;
    }
}

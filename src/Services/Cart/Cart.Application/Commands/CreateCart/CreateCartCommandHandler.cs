using Cart.Domain;
using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using MediatR;

namespace Cart.Application.Commands.CreateCart;

public sealed class CreateCartCommandHandler(IEventStore eventStore)
    : IRequestHandler<CreateCartCommand, Guid>
{
    public async Task<Guid> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var cart = CartAggregate.Create(request.CustomerId);
        await eventStore.SaveNewAsync(cart, CartAggregate.StreamType, cancellationToken);
        return cart.Id;
    }
}

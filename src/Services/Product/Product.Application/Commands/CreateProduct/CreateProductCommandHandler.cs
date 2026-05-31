using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using MediatR;
using Product.Domain;

namespace Product.Application.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(IEventStore eventStore)
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = ProductAggregate.Create(request.Name, request.Price);
        await eventStore.SaveNewAsync(product, ProductAggregate.StreamType, ct);
        return product.Id;
    }
}

using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using MediatR;
using Product.Domain;

namespace Product.Application.Commands.UpdateProductPrice;

public sealed class UpdateProductPriceCommandHandler(IEventStore eventStore)
    : IRequestHandler<UpdateProductPriceCommand>
{
    public async Task Handle(UpdateProductPriceCommand request, CancellationToken ct)
    {
        var product = await eventStore.LoadAsync(request.ProductId, ProductAggregate.StreamType, ProductAggregate.Load, ct)
            ?? throw new KeyNotFoundException($"Product {request.ProductId} not found.");
        product.UpdatePrice(request.NewPrice);
        await eventStore.SaveAsync(product, ProductAggregate.StreamType, ct);
    }
}

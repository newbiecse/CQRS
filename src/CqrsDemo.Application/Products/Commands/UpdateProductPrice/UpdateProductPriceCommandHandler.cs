using CqrsDemo.Application.Abstractions;
using MediatR;

namespace CqrsDemo.Application.Products.Commands.UpdateProductPrice;

public sealed class UpdateProductPriceCommandHandler(
    IProductWriteRepository writeRepository,
    IDomainEventDispatcher domainEventDispatcher) : IRequestHandler<UpdateProductPriceCommand>
{
    public async Task Handle(UpdateProductPriceCommand request, CancellationToken cancellationToken)
    {
        var product = await writeRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {request.ProductId} was not found.");

        product.UpdatePrice(request.NewPrice);

        await writeRepository.UpdateAsync(product, cancellationToken);
        await domainEventDispatcher.DispatchAsync(product.DomainEvents, cancellationToken);
        product.ClearDomainEvents();
    }
}

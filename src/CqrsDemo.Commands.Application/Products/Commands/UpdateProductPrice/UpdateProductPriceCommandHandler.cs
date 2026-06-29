using CqrsDemo.Commands.Application.Abstractions;
using MediatR;

namespace CqrsDemo.Commands.Application.Products.Commands.UpdateProductPrice;

public sealed class UpdateProductPriceCommandHandler(IProductWriteRepository repository)
    : IRequestHandler<UpdateProductPriceCommand>
{
    public async Task Handle(UpdateProductPriceCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException($"Product {request.ProductId} was not found.");

        product.UpdatePrice(request.NewPrice);
        await repository.UpdateAsync(product, cancellationToken);
    }
}

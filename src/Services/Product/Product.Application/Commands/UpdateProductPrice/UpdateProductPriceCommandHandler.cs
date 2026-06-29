using MediatR;
using Product.Application.Abstractions;
using Product.Domain;

namespace Product.Application.Commands.UpdateProductPrice;

public sealed class UpdateProductPriceCommandHandler(IProductWriteRepository repository)
    : IRequestHandler<UpdateProductPriceCommand>
{
    public async Task Handle(UpdateProductPriceCommand request, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(request.ProductId, ct)
            ?? throw new KeyNotFoundException($"Product {request.ProductId} not found.");
        product.UpdatePrice(request.NewPrice);
        await repository.UpdateAsync(product, ct);
    }
}

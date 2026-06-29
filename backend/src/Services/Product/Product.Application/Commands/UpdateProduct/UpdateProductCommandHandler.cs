using MediatR;
using Product.Application.Abstractions;

namespace Product.Application.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(IProductWriteRepository repository)
    : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(request.ProductId, ct)
            ?? throw new KeyNotFoundException($"Product {request.ProductId} not found.");

        product.UpdateDetails(request.Name, request.Price);
        await repository.UpdateAsync(product, ct);
    }
}

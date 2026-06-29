using MediatR;
using Product.Application.Abstractions;

namespace Product.Application.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(IProductWriteRepository repository)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(request.ProductId, ct)
            ?? throw new KeyNotFoundException($"Product {request.ProductId} not found.");

        product.Delete();
        await repository.UpdateAsync(product, ct);
    }
}

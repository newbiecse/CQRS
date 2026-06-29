using MediatR;
using Product.Application.Abstractions;
using Product.Domain;

namespace Product.Application.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(IProductWriteRepository repository)
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = ProductAggregate.Create(request.Name, request.Price);
        await repository.AddAsync(product, ct);
        return product.Id;
    }
}

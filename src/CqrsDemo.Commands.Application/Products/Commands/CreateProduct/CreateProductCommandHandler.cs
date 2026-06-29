using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Domain.Products;
using MediatR;

namespace CqrsDemo.Commands.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(IProductWriteRepository repository)
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Price);
        await repository.AddAsync(product, cancellationToken);
        return product.Id;
    }
}

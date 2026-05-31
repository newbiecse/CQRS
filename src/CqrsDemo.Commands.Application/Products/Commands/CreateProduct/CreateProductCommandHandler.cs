using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Domain.Products;
using MediatR;

namespace CqrsDemo.Commands.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(IProductWriteUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Price);
        await unitOfWork.SaveNewAsync(product, cancellationToken);
        return product.Id;
    }
}

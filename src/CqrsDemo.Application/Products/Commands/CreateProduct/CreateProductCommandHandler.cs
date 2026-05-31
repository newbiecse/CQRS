using CqrsDemo.Application.Abstractions;
using CqrsDemo.Domain.Products;
using MediatR;

namespace CqrsDemo.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IProductWriteRepository writeRepository,
    IDomainEventDispatcher domainEventDispatcher) : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Price);

        await writeRepository.AddAsync(product, cancellationToken);
        await domainEventDispatcher.DispatchAsync(product.DomainEvents, cancellationToken);
        product.ClearDomainEvents();

        return product.Id;
    }
}

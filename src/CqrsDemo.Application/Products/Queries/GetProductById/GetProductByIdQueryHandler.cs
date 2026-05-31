using CqrsDemo.Application.Abstractions;
using CqrsDemo.Application.Products.ReadModels;
using MediatR;

namespace CqrsDemo.Application.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IProductReadRepository readRepository)
    : IRequestHandler<GetProductByIdQuery, ProductReadModel?>
{
    public Task<ProductReadModel?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return readRepository.GetByIdAsync(request.ProductId, cancellationToken);
    }
}

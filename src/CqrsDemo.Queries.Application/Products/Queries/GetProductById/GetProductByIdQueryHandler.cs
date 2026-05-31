using CqrsDemo.Queries.Application.Abstractions;
using CqrsDemo.Queries.Application.Products.ReadModels;
using MediatR;

namespace CqrsDemo.Queries.Application.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IProductReadRepository readRepository)
    : IRequestHandler<GetProductByIdQuery, ProductReadModel?>
{
    public Task<ProductReadModel?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return readRepository.GetByIdAsync(request.ProductId, cancellationToken);
    }
}

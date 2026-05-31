using CqrsDemo.Application.Abstractions;
using CqrsDemo.Application.Products.ReadModels;
using MediatR;

namespace CqrsDemo.Application.Products.Queries.GetAllProducts;

public sealed class GetAllProductsQueryHandler(IProductReadRepository readRepository)
    : IRequestHandler<GetAllProductsQuery, IReadOnlyList<ProductReadModel>>
{
    public Task<IReadOnlyList<ProductReadModel>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        return readRepository.GetAllAsync(cancellationToken);
    }
}

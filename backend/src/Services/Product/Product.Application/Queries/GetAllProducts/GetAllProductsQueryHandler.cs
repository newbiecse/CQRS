using MediatR;
using Product.Application.Abstractions;
using Product.Application.ReadModels;

namespace Product.Application.Queries.GetAllProducts;

public sealed class GetAllProductsQueryHandler(IProductReadRepository repo)
    : IRequestHandler<GetAllProductsQuery, IReadOnlyList<ProductReadModel>>
{
    public Task<IReadOnlyList<ProductReadModel>> Handle(GetAllProductsQuery request, CancellationToken ct) =>
        repo.GetAllAsync(ct);
}

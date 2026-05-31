using MediatR;
using Product.Application.Abstractions;
using Product.Application.ReadModels;

namespace Product.Application.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IProductReadRepository repo)
    : IRequestHandler<GetProductByIdQuery, ProductReadModel?>
{
    public Task<ProductReadModel?> Handle(GetProductByIdQuery request, CancellationToken ct) =>
        repo.GetByIdAsync(request.ProductId, ct);
}

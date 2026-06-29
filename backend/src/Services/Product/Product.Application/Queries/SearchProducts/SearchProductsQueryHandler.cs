using MediatR;
using Product.Application.Abstractions;
using Product.Application.ReadModels;

namespace Product.Application.Queries.SearchProducts;

public sealed class SearchProductsQueryHandler(IProductSearchReader reader)
    : IRequestHandler<SearchProductsQuery, IReadOnlyList<ProductSearchResult>>
{
    public Task<IReadOnlyList<ProductSearchResult>> Handle(SearchProductsQuery request, CancellationToken ct) =>
        reader.SearchAsync(request.Query, request.Size, ct);
}

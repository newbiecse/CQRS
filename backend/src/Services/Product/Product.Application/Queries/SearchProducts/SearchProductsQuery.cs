using MediatR;
using Product.Application.ReadModels;

namespace Product.Application.Queries.SearchProducts;

public sealed record SearchProductsQuery(string Query, int Size = 20) : IRequest<IReadOnlyList<ProductSearchResult>>;

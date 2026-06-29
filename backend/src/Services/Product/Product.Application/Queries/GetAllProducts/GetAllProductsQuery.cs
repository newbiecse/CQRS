using MediatR;
using Product.Application.ReadModels;

namespace Product.Application.Queries.GetAllProducts;

public sealed record GetAllProductsQuery : IRequest<IReadOnlyList<ProductReadModel>>;

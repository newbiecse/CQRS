using CqrsDemo.Queries.Application.Products.ReadModels;
using MediatR;

namespace CqrsDemo.Queries.Application.Products.Queries.GetAllProducts;

public sealed record GetAllProductsQuery : IRequest<IReadOnlyList<ProductReadModel>>;

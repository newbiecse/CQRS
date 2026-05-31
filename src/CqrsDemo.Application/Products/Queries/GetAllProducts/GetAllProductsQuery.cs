using CqrsDemo.Application.Products.ReadModels;
using MediatR;

namespace CqrsDemo.Application.Products.Queries.GetAllProducts;

public sealed record GetAllProductsQuery : IRequest<IReadOnlyList<ProductReadModel>>;

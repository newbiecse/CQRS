using CqrsDemo.Queries.Application.Products.ReadModels;
using MediatR;

namespace CqrsDemo.Queries.Application.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IRequest<ProductReadModel?>;

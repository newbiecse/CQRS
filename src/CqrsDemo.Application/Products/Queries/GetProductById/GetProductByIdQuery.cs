using CqrsDemo.Application.Products.ReadModels;
using MediatR;

namespace CqrsDemo.Application.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IRequest<ProductReadModel?>;

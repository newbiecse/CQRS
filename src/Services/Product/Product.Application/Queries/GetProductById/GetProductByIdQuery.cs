using MediatR;
using Product.Application.ReadModels;

namespace Product.Application.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IRequest<ProductReadModel?>;

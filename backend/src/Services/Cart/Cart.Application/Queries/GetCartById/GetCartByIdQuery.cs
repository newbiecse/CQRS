using Cart.Application.ReadModels;
using MediatR;

namespace Cart.Application.Queries.GetCartById;

public sealed record GetCartByIdQuery(Guid CartId) : IRequest<CartReadModel?>;

using CqrsDemo.Queries.Application.Carts.ReadModels;
using MediatR;

namespace CqrsDemo.Queries.Application.Carts.Queries.GetCartById;

public sealed record GetCartByIdQuery(Guid CartId) : IRequest<CartReadModel?>;

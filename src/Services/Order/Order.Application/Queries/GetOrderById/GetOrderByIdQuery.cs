using MediatR;
using Order.Application.ReadModels;

namespace Order.Application.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderReadModel?>;

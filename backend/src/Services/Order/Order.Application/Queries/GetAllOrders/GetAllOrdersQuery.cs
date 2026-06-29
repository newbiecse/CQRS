using MediatR;
using Order.Application.ReadModels;

namespace Order.Application.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery : IRequest<IReadOnlyList<OrderReadModel>>;

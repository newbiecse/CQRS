using CqrsDemo.Queries.Application.Orders.ReadModels;
using MediatR;

namespace CqrsDemo.Queries.Application.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery : IRequest<IReadOnlyList<OrderReadModel>>;

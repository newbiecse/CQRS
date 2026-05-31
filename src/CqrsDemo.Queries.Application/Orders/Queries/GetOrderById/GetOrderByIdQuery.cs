using CqrsDemo.Queries.Application.Orders.ReadModels;
using MediatR;

namespace CqrsDemo.Queries.Application.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderReadModel?>;

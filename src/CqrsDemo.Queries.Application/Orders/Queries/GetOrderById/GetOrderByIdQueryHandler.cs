using CqrsDemo.Queries.Application.Abstractions;
using CqrsDemo.Queries.Application.Orders.ReadModels;
using MediatR;

namespace CqrsDemo.Queries.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler(IOrderReadRepository repository)
    : IRequestHandler<GetOrderByIdQuery, OrderReadModel?>
{
    public Task<OrderReadModel?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(request.OrderId, cancellationToken);
}

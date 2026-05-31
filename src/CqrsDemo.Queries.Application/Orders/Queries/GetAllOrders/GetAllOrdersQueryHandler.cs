using CqrsDemo.Queries.Application.Abstractions;
using CqrsDemo.Queries.Application.Orders.ReadModels;
using MediatR;

namespace CqrsDemo.Queries.Application.Orders.Queries.GetAllOrders;

public sealed class GetAllOrdersQueryHandler(IOrderReadRepository repository)
    : IRequestHandler<GetAllOrdersQuery, IReadOnlyList<OrderReadModel>>
{
    public Task<IReadOnlyList<OrderReadModel>> Handle(
        GetAllOrdersQuery request,
        CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);
}

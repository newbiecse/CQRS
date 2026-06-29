using MediatR;
using Order.Application.Abstractions;
using Order.Application.ReadModels;

namespace Order.Application.Queries.GetAllOrders;

public sealed class GetAllOrdersQueryHandler(IOrderReadRepository repository)
    : IRequestHandler<GetAllOrdersQuery, IReadOnlyList<OrderReadModel>>
{
    public Task<IReadOnlyList<OrderReadModel>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);
}

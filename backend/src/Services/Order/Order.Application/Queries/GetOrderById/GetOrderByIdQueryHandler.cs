using MediatR;
using Order.Application.Abstractions;
using Order.Application.ReadModels;

namespace Order.Application.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler(IOrderReadRepository repository)
    : IRequestHandler<GetOrderByIdQuery, OrderReadModel?>
{
    public Task<OrderReadModel?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(request.OrderId, cancellationToken);
}

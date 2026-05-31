using CqrsDemo.Queries.Application.Abstractions;
using CqrsDemo.Queries.Application.Carts.ReadModels;
using MediatR;

namespace CqrsDemo.Queries.Application.Carts.Queries.GetCartById;

public sealed class GetCartByIdQueryHandler(ICartReadRepository repository)
    : IRequestHandler<GetCartByIdQuery, CartReadModel?>
{
    public Task<CartReadModel?> Handle(GetCartByIdQuery request, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(request.CartId, cancellationToken);
}

using Cart.Application.Abstractions;
using Cart.Application.ReadModels;
using MediatR;

namespace Cart.Application.Queries.GetCartById;

public sealed class GetCartByIdQueryHandler(ICartReadRepository repository)
    : IRequestHandler<GetCartByIdQuery, CartReadModel?>
{
    public Task<CartReadModel?> Handle(GetCartByIdQuery request, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(request.CartId, cancellationToken);
}

using MediatR;

namespace Cart.Application.Commands.CreateCart;

public sealed record CreateCartCommand(Guid CustomerId) : IRequest<Guid>;

using MediatR;

namespace Cart.Application.Commands.CheckoutCart;

public sealed record CheckoutCartCommand(Guid CartId) : IRequest<Guid>;

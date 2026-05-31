using MediatR;

namespace CqrsDemo.Commands.Application.Orders.Commands.CheckoutCart;

public sealed record CheckoutCartCommand(Guid CartId) : IRequest<Guid>;

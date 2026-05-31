using MediatR;

namespace CqrsDemo.Commands.Application.Carts.Commands.CreateCart;

public sealed record CreateCartCommand(Guid CustomerId) : IRequest<Guid>;

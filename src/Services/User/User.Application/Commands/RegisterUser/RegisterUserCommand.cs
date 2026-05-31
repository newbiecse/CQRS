using MediatR;

namespace User.Application.Commands.RegisterUser;

public sealed record RegisterUserCommand(string Email, string DisplayName) : IRequest<Guid>;

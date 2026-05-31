using MediatR;

namespace User.Application.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(Guid UserId) : IRequest;

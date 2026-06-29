using MediatR;

namespace User.Application.Commands.ProvisionUser;

public sealed record ProvisionUserCommand(Guid UserId, string Email, string DisplayName) : IRequest;

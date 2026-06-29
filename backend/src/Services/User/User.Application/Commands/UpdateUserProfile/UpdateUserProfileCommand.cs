using MediatR;

namespace User.Application.Commands.UpdateUserProfile;

public sealed record UpdateUserProfileCommand(Guid UserId, string DisplayName) : IRequest;

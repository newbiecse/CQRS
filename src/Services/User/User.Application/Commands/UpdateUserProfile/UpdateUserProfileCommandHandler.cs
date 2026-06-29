using MediatR;
using User.Application.Abstractions;
using User.Domain;

namespace User.Application.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandHandler(IUserWriteRepository repository)
    : IRequestHandler<UpdateUserProfileCommand>
{
    public async Task Handle(UpdateUserProfileCommand request, CancellationToken ct)
    {
        var user = await repository.GetByIdAsync(request.UserId, ct)
            ?? throw new KeyNotFoundException($"User {request.UserId} was not found.");

        user.UpdateProfile(request.DisplayName);
        await repository.UpdateAsync(user, ct);
    }
}

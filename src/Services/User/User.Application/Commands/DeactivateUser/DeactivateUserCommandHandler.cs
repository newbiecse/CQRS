using MediatR;
using User.Application.Abstractions;
using User.Domain;

namespace User.Application.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler(IUserWriteRepository repository)
    : IRequestHandler<DeactivateUserCommand>
{
    public async Task Handle(DeactivateUserCommand request, CancellationToken ct)
    {
        var user = await repository.GetByIdAsync(request.UserId, ct)
            ?? throw new KeyNotFoundException($"User {request.UserId} was not found.");

        user.Deactivate();
        await repository.UpdateAsync(user, ct);
    }
}

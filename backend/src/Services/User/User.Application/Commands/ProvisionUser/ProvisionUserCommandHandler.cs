using MediatR;
using User.Application.Abstractions;
using User.Domain;

namespace User.Application.Commands.ProvisionUser;

public sealed class ProvisionUserCommandHandler(IUserWriteRepository repository)
    : IRequestHandler<ProvisionUserCommand>
{
    public async Task Handle(ProvisionUserCommand request, CancellationToken cancellationToken)
    {
        if (await repository.ExistsAsync(request.UserId, cancellationToken))
            return;

        var email = request.Email.Trim().ToLowerInvariant();
        if (await repository.ExistsByEmailAsync(email, cancellationToken))
            return;

        var user = UserAggregate.Register(request.UserId, email, request.DisplayName);
        await repository.AddAsync(user, cancellationToken);
    }
}

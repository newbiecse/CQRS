using MediatR;
using User.Application.Abstractions;
using User.Domain;

namespace User.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(IUserWriteRepository repository)
    : IRequestHandler<RegisterUserCommand, Guid>
{
    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await repository.ExistsByEmailAsync(email, ct))
            throw new InvalidOperationException($"Email '{email}' is already registered.");

        var user = UserAggregate.Register(Guid.NewGuid(), email, request.DisplayName);
        await repository.AddAsync(user, ct);
        return user.Id;
    }
}

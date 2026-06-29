using MediatR;
using User.Application.Abstractions;
using User.Domain;

namespace User.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(IUserWriteRepository repository)
    : IRequestHandler<RegisterUserCommand, Guid>
{
    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var user = UserAggregate.Register(request.Email, request.DisplayName);
        await repository.AddAsync(user, ct);
        return user.Id;
    }
}

using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using MediatR;
using User.Domain;

namespace User.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(IEventStore eventStore)
    : IRequestHandler<RegisterUserCommand, Guid>
{
    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var user = UserAggregate.Register(request.Email, request.DisplayName);
        await eventStore.SaveNewAsync(user, UserAggregate.StreamType, ct);
        return user.Id;
    }
}

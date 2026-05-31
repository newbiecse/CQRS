using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using MediatR;
using User.Domain;

namespace User.Application.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler(IEventStore eventStore)
    : IRequestHandler<DeactivateUserCommand>
{
    public async Task Handle(DeactivateUserCommand request, CancellationToken ct)
    {
        var user = await eventStore.LoadAsync(
            request.UserId,
            UserAggregate.StreamType,
            UserAggregate.Load,
            ct) ?? throw new KeyNotFoundException($"User {request.UserId} was not found.");

        user.Deactivate();
        await eventStore.SaveAsync(user, UserAggregate.StreamType, ct);
    }
}

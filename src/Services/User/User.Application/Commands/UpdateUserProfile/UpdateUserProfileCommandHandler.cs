using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using MediatR;
using User.Domain;

namespace User.Application.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandHandler(IEventStore eventStore)
    : IRequestHandler<UpdateUserProfileCommand>
{
    public async Task Handle(UpdateUserProfileCommand request, CancellationToken ct)
    {
        var user = await eventStore.LoadAsync(
            request.UserId,
            UserAggregate.StreamType,
            UserAggregate.Load,
            ct) ?? throw new KeyNotFoundException($"User {request.UserId} was not found.");

        user.UpdateProfile(request.DisplayName);
        await eventStore.SaveAsync(user, UserAggregate.StreamType, ct);
    }
}

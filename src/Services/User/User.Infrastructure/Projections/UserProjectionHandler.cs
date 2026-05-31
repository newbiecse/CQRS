using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Serialization;
using CqrsDemo.Contracts.Users;
using User.Application.Abstractions;
using User.Application.ReadModels;

namespace User.Infrastructure.Projections;

public sealed class UserProjectionHandler(IUserReadRepository repo)
{
    public async Task HandleAsync(string eventType, string payload, CancellationToken ct)
    {
        switch (eventType)
        {
            case IntegrationEventTypes.UserRegistered:
                var registered = IntegrationEventSerializer.Deserialize<UserRegisteredIntegrationEvent>(payload);
                await repo.UpsertAsync(new UserReadModel
                {
                    Id = registered.UserId,
                    Email = registered.Email,
                    DisplayName = registered.DisplayName,
                    IsActive = true,
                    RegisteredAt = registered.RegisteredAt,
                    LastUpdatedAt = registered.RegisteredAt
                }, ct);
                break;
            case IntegrationEventTypes.UserProfileUpdated:
                var updated = IntegrationEventSerializer.Deserialize<UserProfileUpdatedIntegrationEvent>(payload);
                var existing = await repo.GetByIdAsync(updated.UserId, ct);
                if (existing is null) return;
                await repo.UpsertAsync(new UserReadModel
                {
                    Id = existing.Id,
                    Email = updated.Email,
                    DisplayName = updated.DisplayName,
                    IsActive = existing.IsActive,
                    RegisteredAt = existing.RegisteredAt,
                    LastUpdatedAt = updated.UpdatedAt
                }, ct);
                break;
            case IntegrationEventTypes.UserDeactivated:
                var deactivated = IntegrationEventSerializer.Deserialize<UserDeactivatedIntegrationEvent>(payload);
                var user = await repo.GetByIdAsync(deactivated.UserId, ct);
                if (user is null) return;
                await repo.UpsertAsync(new UserReadModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    IsActive = false,
                    RegisteredAt = user.RegisteredAt,
                    LastUpdatedAt = deactivated.DeactivatedAt
                }, ct);
                break;
        }
    }
}

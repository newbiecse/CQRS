using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using User.Application.Abstractions;
using User.Domain;

namespace User.Infrastructure.Persistence.Write;

public sealed class SqlUserWriteRepository(
    UserWriteDbContext db,
    IIntegrationEventMapper integrationEventMapper) : IUserWriteRepository
{
    public async Task AddAsync(UserAggregate user, CancellationToken cancellationToken = default)
    {
        db.Users.Add(ToEntity(user));
        db.AddOutboxMessages(integrationEventMapper, user.DomainEvents);
        user.ClearDomainEvents();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        return entity is null ? null : ToAggregate(entity);
    }

    public async Task UpdateAsync(UserAggregate user, CancellationToken cancellationToken = default)
    {
        var entity = await db.Users.FindAsync([user.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"User {user.Id} was not found.");

        entity.Email = user.Email;
        entity.DisplayName = user.DisplayName;
        entity.IsActive = user.IsActive;
        entity.RegisteredAt = user.RegisteredAt;

        db.AddOutboxMessages(integrationEventMapper, user.DomainEvents);
        user.ClearDomainEvents();
        await db.SaveChangesAsync(cancellationToken);
    }

    private static UserWriteEntity ToEntity(UserAggregate user) =>
        new()
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            IsActive = user.IsActive,
            RegisteredAt = user.RegisteredAt
        };

    private static UserAggregate ToAggregate(UserWriteEntity entity) =>
        UserAggregate.Restore(entity.Id, entity.Email, entity.DisplayName, entity.IsActive, entity.RegisteredAt);
}

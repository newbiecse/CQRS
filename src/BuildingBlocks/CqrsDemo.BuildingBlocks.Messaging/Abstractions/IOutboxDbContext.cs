using CqrsDemo.BuildingBlocks.Messaging.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CqrsDemo.BuildingBlocks.Messaging.Abstractions;

public interface IOutboxDbContext
{
    DbSet<OutboxMessageEntity> OutboxMessages { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

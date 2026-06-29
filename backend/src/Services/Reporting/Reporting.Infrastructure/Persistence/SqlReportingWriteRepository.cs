using Microsoft.EntityFrameworkCore;
using Reporting.Application.Abstractions;

namespace Reporting.Infrastructure.Persistence;

public sealed class SqlReportingWriteRepository(ReportingDbContext db) : IReportingWriteRepository
{
    public async Task UpsertUserProfileAsync(
        Guid userId,
        string email,
        string displayName,
        bool isActive,
        DateTime updatedAt,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.UserProfiles.FindAsync([userId], cancellationToken);
        if (entity is null)
        {
            entity = new UserProfileReportEntity { UserId = userId };
            db.UserProfiles.Add(entity);
        }

        entity.Email = email;
        entity.DisplayName = displayName;
        entity.IsActive = isActive;
        entity.LastUpdatedAt = updatedAt;

        await db.SaveChangesAsync(cancellationToken);

        await BackfillOrderFactsUserInfoAsync(userId, email, displayName, updatedAt, cancellationToken);
    }

    public async Task UpsertOrderFactAsync(
        Guid orderId,
        Guid userId,
        string? userEmail,
        string? userDisplayName,
        decimal totalAmount,
        string status,
        DateTime orderCreatedAt,
        CancellationToken cancellationToken = default)
    {
        var profile = await db.UserProfiles.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        var entity = await db.OrderFacts.FindAsync([orderId], cancellationToken);
        if (entity is null)
        {
            entity = new OrderReportFactEntity { OrderId = orderId };
            db.OrderFacts.Add(entity);
        }

        entity.UserId = userId;
        entity.UserEmail = userEmail ?? profile?.Email;
        entity.UserDisplayName = userDisplayName ?? profile?.DisplayName;
        entity.TotalAmount = totalAmount;
        entity.Status = status;
        entity.OrderCreatedAt = orderCreatedAt;
        entity.LastUpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateUserProfileAsync(Guid userId, DateTime deactivatedAt, CancellationToken cancellationToken = default)
    {
        var entity = await db.UserProfiles.FindAsync([userId], cancellationToken);
        if (entity is null) return;

        entity.IsActive = false;
        entity.LastUpdatedAt = deactivatedAt;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateOrderStatusAsync(
        Guid orderId,
        string status,
        DateTime updatedAt,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.OrderFacts.FindAsync([orderId], cancellationToken);
        if (entity is null) return;

        entity.Status = status;
        entity.LastUpdatedAt = updatedAt;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceOrderLineFactsAsync(
        Guid orderId,
        IReadOnlyList<OrderLineFactInput> lines,
        DateTime orderCreatedAt,
        CancellationToken cancellationToken = default)
    {
        var existing = await db.OrderLineFacts.Where(l => l.OrderId == orderId).ToListAsync(cancellationToken);
        if (existing.Count > 0)
            db.OrderLineFacts.RemoveRange(existing);

        foreach (var line in lines)
        {
            db.OrderLineFacts.Add(new OrderLineFactEntity
            {
                OrderId = orderId,
                ProductId = line.ProductId,
                ProductName = line.ProductName,
                UnitPrice = line.UnitPrice,
                Quantity = line.Quantity,
                LineTotal = line.LineTotal,
                OrderCreatedAt = orderCreatedAt
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task BackfillOrderFactsUserInfoAsync(
        Guid userId,
        string email,
        string displayName,
        DateTime updatedAt,
        CancellationToken cancellationToken)
    {
        var orders = await db.OrderFacts
            .Where(o => o.UserId == userId && (o.UserEmail == null || o.UserDisplayName == null))
            .ToListAsync(cancellationToken);

        if (orders.Count == 0) return;

        foreach (var order in orders)
        {
            order.UserEmail ??= email;
            order.UserDisplayName ??= displayName;
            order.LastUpdatedAt = updatedAt;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}

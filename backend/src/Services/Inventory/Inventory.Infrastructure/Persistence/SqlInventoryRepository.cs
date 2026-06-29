using Inventory.Application.Abstractions;
using Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public sealed class SqlInventoryRepository(InventoryDbContext db) : IInventoryRepository
{
    public async Task<InventoryItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var entity = await db.InventoryItems.AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductId == productId, cancellationToken);
        return entity is null ? null : ToAggregate(entity);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await db.InventoryItems.AsNoTracking().OrderBy(i => i.ProductName).ToListAsync(cancellationToken);
        return entities.Select(ToAggregate).ToList();
    }

    public Task<bool> HasReservationForOrderAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        db.OrderReservations.AnyAsync(r => r.OrderId == orderId, cancellationToken);

    public async Task<IReadOnlyList<OrderReservationLine>> GetReservationsForOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var rows = await db.OrderReservations.AsNoTracking()
            .Where(r => r.OrderId == orderId)
            .ToListAsync(cancellationToken);
        return rows.Select(r => new OrderReservationLine(r.ProductId, r.Quantity)).ToList();
    }

    public async Task InitializeAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        db.InventoryItems.Add(ToEntity(item));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(InventoryItem item, CancellationToken cancellationToken = default)
    {
        var entity = await db.InventoryItems.FindAsync([item.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"Inventory for product {item.Id} was not found.");

        entity.ProductName = item.ProductName;
        entity.OnHand = item.OnHand;
        entity.Reserved = item.Reserved;
        entity.LastUpdatedAt = item.LastUpdatedAt;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReserveForOrderAsync(
        Guid orderId,
        IReadOnlyList<StockLineRequest> lines,
        CancellationToken cancellationToken = default)
    {
        if (await db.OrderReservations.AnyAsync(r => r.OrderId == orderId, cancellationToken))
            return;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var grouped = lines
            .GroupBy(l => l.ProductId)
            .Select(g => new StockLineRequest(g.Key, g.Sum(x => x.Quantity)))
            .ToList();

        var productIds = grouped.Select(l => l.ProductId).ToList();
        var items = await db.InventoryItems
            .Where(i => productIds.Contains(i.ProductId))
            .ToListAsync(cancellationToken);

        if (items.Count != productIds.Count)
        {
            var missing = productIds.Except(items.Select(i => i.ProductId)).First();
            throw new KeyNotFoundException($"Inventory for product {missing} was not found.");
        }

        foreach (var line in grouped)
        {
            var entity = items.First(i => i.ProductId == line.ProductId);
            var aggregate = ToAggregate(entity);
            aggregate.Reserve(line.Quantity);
            entity.OnHand = aggregate.OnHand;
            entity.Reserved = aggregate.Reserved;
            entity.LastUpdatedAt = aggregate.LastUpdatedAt;

            db.OrderReservations.Add(new OrderReservationEntity
            {
                OrderId = orderId,
                ProductId = line.ProductId,
                Quantity = line.Quantity
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task ReleaseForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var reservations = await db.OrderReservations
            .Where(r => r.OrderId == orderId)
            .ToListAsync(cancellationToken);
        if (reservations.Count == 0) return;

        foreach (var reservation in reservations)
        {
            var entity = await db.InventoryItems.FindAsync([reservation.ProductId], cancellationToken)
                ?? throw new KeyNotFoundException($"Inventory for product {reservation.ProductId} was not found.");

            var aggregate = ToAggregate(entity);
            aggregate.Release(reservation.Quantity);
            entity.Reserved = aggregate.Reserved;
            entity.LastUpdatedAt = aggregate.LastUpdatedAt;
        }

        db.OrderReservations.RemoveRange(reservations);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task ConfirmForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var reservations = await db.OrderReservations
            .Where(r => r.OrderId == orderId)
            .ToListAsync(cancellationToken);
        if (reservations.Count == 0) return;

        foreach (var reservation in reservations)
        {
            var entity = await db.InventoryItems.FindAsync([reservation.ProductId], cancellationToken)
                ?? throw new KeyNotFoundException($"Inventory for product {reservation.ProductId} was not found.");

            var aggregate = ToAggregate(entity);
            aggregate.Confirm(reservation.Quantity);
            entity.OnHand = aggregate.OnHand;
            entity.Reserved = aggregate.Reserved;
            entity.LastUpdatedAt = aggregate.LastUpdatedAt;
        }

        db.OrderReservations.RemoveRange(reservations);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var entity = await db.InventoryItems.FindAsync([productId], cancellationToken);
        if (entity is null) return;

        if (entity.Reserved > 0)
            throw new InvalidOperationException($"Cannot delete inventory for product {productId} while stock is reserved.");

        db.InventoryItems.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static InventoryItem ToAggregate(InventoryItemEntity entity) =>
        InventoryItem.Restore(entity.ProductId, entity.ProductName, entity.OnHand, entity.Reserved, entity.LastUpdatedAt);

    private static InventoryItemEntity ToEntity(InventoryItem item) =>
        new()
        {
            ProductId = item.Id,
            ProductName = item.ProductName,
            OnHand = item.OnHand,
            Reserved = item.Reserved,
            LastUpdatedAt = item.LastUpdatedAt
        };
}

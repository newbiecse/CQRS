using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions;
using Order.Domain;

namespace Order.Infrastructure.Persistence.Write;

public sealed class SqlOrderWriteRepository(
    OrderWriteDbContext db,
    IIntegrationEventMapper integrationEventMapper) : IOrderWriteRepository
{
    public async Task AddAsync(OrderAggregate order, CancellationToken cancellationToken = default)
    {
        db.Orders.Add(ToEntity(order));
        db.OrderLines.AddRange(ToLineEntities(order));
        db.AddOutboxMessages(integrationEventMapper, order.DomainEvents);
        order.ClearDomainEvents();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (entity is null) return null;

        var lines = await db.OrderLines.AsNoTracking()
            .Where(l => l.OrderId == id)
            .ToListAsync(cancellationToken);

        return ToAggregate(entity, lines);
    }

    public async Task UpdateAsync(OrderAggregate order, CancellationToken cancellationToken = default)
    {
        var entity = await db.Orders.FindAsync([order.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"Order {order.Id} was not found.");

        entity.CustomerId = order.CustomerId;
        entity.CartId = order.CartId;
        entity.Status = order.Status.ToString();
        entity.TotalAmount = order.TotalAmount;
        entity.CreatedAt = order.CreatedAt;

        var existingLines = await db.OrderLines.Where(l => l.OrderId == order.Id).ToListAsync(cancellationToken);
        db.OrderLines.RemoveRange(existingLines);
        db.OrderLines.AddRange(ToLineEntities(order));

        db.AddOutboxMessages(integrationEventMapper, order.DomainEvents);
        order.ClearDomainEvents();
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        db.Orders.AnyAsync(o => o.Id == orderId, cancellationToken);

    private static OrderWriteEntity ToEntity(OrderAggregate order) =>
        new()
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CartId = order.CartId,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt
        };

    private static IEnumerable<OrderLineWriteEntity> ToLineEntities(OrderAggregate order) =>
        order.Lines.Select(l => new OrderLineWriteEntity
        {
            OrderId = order.Id,
            ProductId = l.ProductId,
            ProductName = l.ProductName,
            UnitPrice = l.UnitPrice,
            Quantity = l.Quantity
        });

    private static OrderAggregate ToAggregate(OrderWriteEntity entity, IReadOnlyList<OrderLineWriteEntity> lines)
    {
        var orderLines = lines.Select(l => new OrderLine
        {
            ProductId = l.ProductId,
            ProductName = l.ProductName,
            UnitPrice = l.UnitPrice,
            Quantity = l.Quantity
        }).ToList();

        return OrderAggregate.Restore(
            entity.Id,
            entity.CustomerId,
            entity.CartId,
            Enum.Parse<OrderStatus>(entity.Status),
            entity.TotalAmount,
            entity.CreatedAt,
            orderLines);
    }
}

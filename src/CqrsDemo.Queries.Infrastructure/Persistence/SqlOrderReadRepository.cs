using CqrsDemo.Contracts.Common;
using CqrsDemo.Queries.Application.Abstractions;
using CqrsDemo.Queries.Application.Orders.ReadModels;
using CqrsDemo.Queries.Infrastructure.Persistence.Read;
using Microsoft.EntityFrameworkCore;

namespace CqrsDemo.Queries.Infrastructure.Persistence;

public sealed class SqlOrderReadRepository(ReadDbContext dbContext) : IOrderReadRepository
{
    public async Task UpsertAsync(OrderReadModel order, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == order.Id, cancellationToken);

        if (entity is null)
        {
            entity = new OrderReadEntity { Id = order.Id };
            dbContext.Orders.Add(entity);
        }
        else
        {
            dbContext.OrderLines.RemoveRange(entity.Lines);
        }

        entity.CustomerId = order.CustomerId;
        entity.CartId = order.CartId;
        entity.Status = order.Status;
        entity.TotalAmount = order.TotalAmount;
        entity.PaymentId = order.PaymentId;
        entity.CreatedAt = order.CreatedAt;
        entity.LastUpdatedAt = order.LastUpdatedAt;
        entity.Lines = order.Lines.Select(line => new OrderLineReadEntity
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductId = line.ProductId,
            ProductName = line.ProductName,
            UnitPrice = line.UnitPrice,
            Quantity = line.Quantity
        }).ToList();

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return entity is null ? null : ToReadModel(entity);
    }

    public async Task<IReadOnlyList<OrderReadModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Lines)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(ToReadModel).ToList();
    }

    private static OrderReadModel ToReadModel(OrderReadEntity entity) =>
        new()
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            CartId = entity.CartId,
            Status = entity.Status,
            TotalAmount = entity.TotalAmount,
            PaymentId = entity.PaymentId,
            CreatedAt = entity.CreatedAt,
            LastUpdatedAt = entity.LastUpdatedAt,
            Lines = entity.Lines.Select(l => new OrderLineDto(
                l.ProductId, l.ProductName, l.UnitPrice, l.Quantity)).ToList()
        };
}

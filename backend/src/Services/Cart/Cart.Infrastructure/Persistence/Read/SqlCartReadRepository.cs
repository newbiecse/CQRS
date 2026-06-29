using Cart.Application.Abstractions;
using Cart.Application.ReadModels;
using CqrsDemo.Contracts.Common;
using Microsoft.EntityFrameworkCore;

namespace Cart.Infrastructure.Persistence.Read;

public sealed class SqlCartReadRepository(CartReadDbContext dbContext) : ICartReadRepository
{
    public async Task UpsertAsync(CartReadModel cart, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Carts
            .Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == cart.Id, cancellationToken);

        if (entity is null)
        {
            entity = new CartReadEntity { Id = cart.Id };
            dbContext.Carts.Add(entity);
        }
        else
        {
            dbContext.CartLines.RemoveRange(entity.Lines);
        }

        entity.CustomerId = cart.CustomerId;
        entity.Status = cart.Status;
        entity.Subtotal = cart.Subtotal;
        entity.CreatedAt = cart.CreatedAt;
        entity.LastUpdatedAt = cart.LastUpdatedAt;
        entity.Lines = cart.Items.Select(line => new CartLineReadEntity
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            ProductId = line.ProductId,
            ProductName = line.ProductName,
            UnitPrice = line.UnitPrice,
            Quantity = line.Quantity
        }).ToList();

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CartReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Carts.AsNoTracking().Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        return entity is null ? null : ToReadModel(entity);
    }

    private static CartReadModel ToReadModel(CartReadEntity entity) => new()
    {
        Id = entity.Id,
        CustomerId = entity.CustomerId,
        Status = entity.Status,
        Subtotal = entity.Subtotal,
        CreatedAt = entity.CreatedAt,
        LastUpdatedAt = entity.LastUpdatedAt,
        Items = entity.Lines.Select(l => new OrderLineDto(l.ProductId, l.ProductName, l.UnitPrice, l.Quantity)).ToList()
    };
}

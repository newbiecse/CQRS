using CqrsDemo.BuildingBlocks.Domain;
using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Domain.Carts;
using Microsoft.EntityFrameworkCore;

namespace CqrsDemo.Commands.Infrastructure.Persistence.Write;

public sealed class SqlCartWriteRepository(
    WriteDbContext db,
    IIntegrationEventMapper integrationEventMapper) : ICartWriteRepository
{
    public async Task AddAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        db.Carts.Add(ToEntity(cart));
        db.CartItems.AddRange(ToItemEntities(cart));
        db.AddOutboxMessages(integrationEventMapper, cart.DomainEvents);
        cart.ClearDomainEvents();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Carts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null) return null;

        var items = await db.CartItems.AsNoTracking()
            .Where(i => i.CartId == id)
            .ToListAsync(cancellationToken);

        return ToAggregate(entity, items);
    }

    public async Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        var entity = await db.Carts.FindAsync([cart.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"Cart {cart.Id} was not found.");

        entity.CustomerId = cart.CustomerId;
        entity.Status = cart.Status.ToString();
        entity.CreatedAt = cart.CreatedAt;

        var existingItems = await db.CartItems.Where(i => i.CartId == cart.Id).ToListAsync(cancellationToken);
        db.CartItems.RemoveRange(existingItems);
        db.CartItems.AddRange(ToItemEntities(cart));

        db.AddOutboxMessages(integrationEventMapper, cart.DomainEvents);
        cart.ClearDomainEvents();
        await db.SaveChangesAsync(cancellationToken);
    }

    private static CartWriteEntity ToEntity(Cart cart) =>
        new()
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            Status = cart.Status.ToString(),
            CreatedAt = cart.CreatedAt
        };

    private static IEnumerable<CartItemWriteEntity> ToItemEntities(Cart cart) =>
        cart.Items.Select(i => new CartItemWriteEntity
        {
            CartId = cart.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity
        });

    private static Cart ToAggregate(CartWriteEntity entity, IReadOnlyList<CartItemWriteEntity> items)
    {
        var lines = items.Select(i => new OrderLine
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity
        }).ToList();

        return Cart.Restore(
            entity.Id,
            entity.CustomerId,
            Enum.Parse<CartStatus>(entity.Status),
            entity.CreatedAt,
            lines);
    }
}

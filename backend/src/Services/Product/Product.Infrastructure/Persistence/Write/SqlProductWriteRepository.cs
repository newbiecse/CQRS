using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Product.Application.Abstractions;
using Product.Domain;

namespace Product.Infrastructure.Persistence.Write;

public sealed class SqlProductWriteRepository(
    ProductWriteDbContext db,
    IIntegrationEventMapper integrationEventMapper) : IProductWriteRepository
{
    public async Task AddAsync(ProductAggregate product, CancellationToken cancellationToken = default)
    {
        db.Products.Add(ToEntity(product));
        db.AddOutboxMessages(integrationEventMapper, product.DomainEvents);
        product.ClearDomainEvents();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return entity is null ? null : ToAggregate(entity);
    }

    public async Task UpdateAsync(ProductAggregate product, CancellationToken cancellationToken = default)
    {
        var entity = await db.Products.FindAsync([product.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"Product {product.Id} was not found.");

        entity.Name = product.Name;
        entity.Price = product.Price;
        db.AddOutboxMessages(integrationEventMapper, product.DomainEvents);
        product.ClearDomainEvents();
        await db.SaveChangesAsync(cancellationToken);
    }

    private static ProductWriteEntity ToEntity(ProductAggregate product) =>
        new()
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            CreatedAt = product.CreatedAt
        };

    private static ProductAggregate ToAggregate(ProductWriteEntity entity) =>
        ProductAggregate.Restore(entity.Id, entity.Name, entity.Price, entity.CreatedAt);
}

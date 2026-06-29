using CqrsDemo.BuildingBlocks.Messaging;
using CqrsDemo.BuildingBlocks.Messaging.Abstractions;
using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace CqrsDemo.Commands.Infrastructure.Persistence.Write;

public sealed class SqlProductWriteRepository(
    WriteDbContext db,
    IIntegrationEventMapper integrationEventMapper) : IProductWriteRepository
{
    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        db.Products.Add(ToEntity(product));
        db.AddOutboxMessages(integrationEventMapper, product.DomainEvents);
        product.ClearDomainEvents();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return entity is null ? null : ToAggregate(entity);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var entity = await db.Products.FindAsync([product.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"Product {product.Id} was not found.");

        entity.Name = product.Name;
        entity.Price = product.Price;
        db.AddOutboxMessages(integrationEventMapper, product.DomainEvents);
        product.ClearDomainEvents();
        await db.SaveChangesAsync(cancellationToken);
    }

    private static ProductWriteEntity ToEntity(Product product) =>
        new()
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            CreatedAt = product.CreatedAt
        };

    private static Product ToAggregate(ProductWriteEntity entity) =>
        Product.Restore(entity.Id, entity.Name, entity.Price, entity.CreatedAt);
}

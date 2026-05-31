using CqrsDemo.Application.Abstractions;
using CqrsDemo.Domain.Products;
using CqrsDemo.Infrastructure.Persistence.Write;
using Microsoft.EntityFrameworkCore;

namespace CqrsDemo.Infrastructure.Persistence;

public sealed class SqlProductWriteRepository(WriteDbContext dbContext) : IProductWriteRepository
{
    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        dbContext.Products.Add(ToEntity(product));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return entity is null ? null : ToDomain(entity);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Product {product.Id} was not found in the write database.");
        }

        entity.Name = product.Name;
        entity.Price = product.Price;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ProductWriteEntity ToEntity(Product product) =>
        new()
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            CreatedAt = product.CreatedAt
        };

    private static Product ToDomain(ProductWriteEntity entity) =>
        Product.Rehydrate(entity.Id, entity.Name, entity.Price, entity.CreatedAt);
}

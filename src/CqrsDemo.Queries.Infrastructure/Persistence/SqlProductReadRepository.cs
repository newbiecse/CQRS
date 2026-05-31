using CqrsDemo.Queries.Application.Abstractions;
using CqrsDemo.Queries.Application.Products.ReadModels;
using CqrsDemo.Queries.Infrastructure.Persistence.Read;
using Microsoft.EntityFrameworkCore;

namespace CqrsDemo.Queries.Infrastructure.Persistence;

public sealed class SqlProductReadRepository(ReadDbContext dbContext) : IProductReadRepository
{
    public async Task UpsertAsync(ProductReadModel product, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);

        if (existing is null)
        {
            dbContext.Products.Add(ToEntity(product));
        }
        else
        {
            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.CreatedAt = product.CreatedAt;
            existing.LastUpdatedAt = product.LastUpdatedAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return entity is null ? null : ToReadModel(entity);
    }

    public async Task<IReadOnlyList<ProductReadModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(ToReadModel).ToList();
    }

    private static ProductReadEntity ToEntity(ProductReadModel model) =>
        new()
        {
            Id = model.Id,
            Name = model.Name,
            Price = model.Price,
            CreatedAt = model.CreatedAt,
            LastUpdatedAt = model.LastUpdatedAt
        };

    private static ProductReadModel ToReadModel(ProductReadEntity entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Price = entity.Price,
            CreatedAt = entity.CreatedAt,
            LastUpdatedAt = entity.LastUpdatedAt
        };
}

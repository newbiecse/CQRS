using Microsoft.EntityFrameworkCore;
using Product.Application.Abstractions;
using Product.Application.ReadModels;

namespace Product.Infrastructure.Persistence.Read;

public sealed class SqlProductReadRepository(ProductReadDbContext db) : IProductReadRepository
{
    public async Task UpsertAsync(ProductReadModel product, CancellationToken ct = default)
    {
        var e = await db.Products.FindAsync([product.Id], ct);
        if (e is null) { e = new ProductReadEntity { Id = product.Id }; db.Products.Add(e); }
        e.Name = product.Name; e.Price = product.Price;
        e.CreatedAt = product.CreatedAt; e.LastUpdatedAt = product.LastUpdatedAt;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.Products.FindAsync([id], ct);
        if (entity is null) return;
        db.Products.Remove(entity);
        await db.SaveChangesAsync(ct);
    }

    public async Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var e = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return e is null ? null : Map(e);
    }

    public async Task<IReadOnlyList<ProductReadModel>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await db.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    private static ProductReadModel Map(ProductReadEntity e) => new()
    {
        Id = e.Id, Name = e.Name, Price = e.Price, CreatedAt = e.CreatedAt, LastUpdatedAt = e.LastUpdatedAt
    };
}

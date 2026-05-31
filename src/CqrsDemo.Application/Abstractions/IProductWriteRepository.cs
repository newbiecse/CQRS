using CqrsDemo.Domain.Products;

namespace CqrsDemo.Application.Abstractions;

public interface IProductWriteRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
}

using Product.Domain;

namespace Product.Application.Abstractions;

public interface IProductWriteRepository
{
    Task AddAsync(ProductAggregate product, CancellationToken cancellationToken = default);
    Task<ProductAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProductAggregate product, CancellationToken cancellationToken = default);
}

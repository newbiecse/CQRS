using CqrsDemo.Domain.Products;

namespace CqrsDemo.Commands.Application.Abstractions;

public interface IProductWriteUnitOfWork
{
    Task SaveNewAsync(Product product, CancellationToken cancellationToken = default);
    Task SaveUpdatedAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoredEventDto>> GetEventHistoryAsync(Guid streamId, CancellationToken cancellationToken = default);
}

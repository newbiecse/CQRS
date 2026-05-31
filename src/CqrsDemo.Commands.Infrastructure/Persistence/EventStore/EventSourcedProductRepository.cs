using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Domain.Products;

namespace CqrsDemo.Commands.Infrastructure.Persistence.EventStore;

public sealed class EventSourcedProductRepository(IEventStore eventStore) : IProductWriteUnitOfWork
{
    public Task SaveNewAsync(Product product, CancellationToken cancellationToken = default) =>
        eventStore.SaveNewAsync(product, Product.StreamType, cancellationToken);

    public Task SaveUpdatedAsync(Product product, CancellationToken cancellationToken = default) =>
        eventStore.SaveAsync(product, Product.StreamType, cancellationToken);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        eventStore.LoadAsync(id, Product.StreamType, Product.LoadFromHistory, cancellationToken);

    public Task<IReadOnlyList<StoredEventDto>> GetEventHistoryAsync(
        Guid streamId,
        CancellationToken cancellationToken = default) =>
        eventStore.GetHistoryAsync(streamId, Product.StreamType, cancellationToken);
}

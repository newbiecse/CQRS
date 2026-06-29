using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Products;
using CqrsDemo.Contracts.Serialization;
using Product.Application.Abstractions;
using Product.Application.ReadModels;

namespace Product.Infrastructure.Projections;

public sealed class ProductProjectionHandler(IProductReadRepository repo)
{
    public async Task HandleAsync(string eventType, string payload, CancellationToken ct)
    {
        switch (eventType)
        {
            case IntegrationEventTypes.ProductCreated:
                var c = IntegrationEventSerializer.Deserialize<ProductCreatedIntegrationEvent>(payload);
                await repo.UpsertAsync(new ProductReadModel
                {
                    Id = c.ProductId, Name = c.Name, Price = c.Price,
                    CreatedAt = c.CreatedAt, LastUpdatedAt = c.CreatedAt
                }, ct);
                break;
            case IntegrationEventTypes.ProductPriceUpdated:
                var u = IntegrationEventSerializer.Deserialize<ProductPriceUpdatedIntegrationEvent>(payload);
                var existing = await repo.GetByIdAsync(u.ProductId, ct);
                if (existing is null) return;
                await repo.UpsertAsync(new ProductReadModel
                {
                    Id = existing.Id, Name = existing.Name, Price = u.NewPrice,
                    CreatedAt = existing.CreatedAt, LastUpdatedAt = u.UpdatedAt
                }, ct);
                break;
        }
    }
}

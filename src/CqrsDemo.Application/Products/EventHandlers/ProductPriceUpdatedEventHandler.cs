using CqrsDemo.Application.Abstractions;
using CqrsDemo.Application.Products.ReadModels;
using CqrsDemo.Domain.Products.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CqrsDemo.Application.Products.EventHandlers;

public sealed class ProductPriceUpdatedEventHandler(
    IProductReadRepository readRepository,
    ILogger<ProductPriceUpdatedEventHandler> logger) : INotificationHandler<ProductPriceUpdatedEvent>
{
    public async Task Handle(ProductPriceUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Projecting ProductPriceUpdatedEvent for product {ProductId}: {OldPrice} -> {NewPrice}",
            notification.ProductId,
            notification.OldPrice,
            notification.NewPrice);

        var existing = await readRepository.GetByIdAsync(notification.ProductId, cancellationToken);
        if (existing is null)
        {
            logger.LogWarning(
                "Read model not found for product {ProductId}; skipping projection.",
                notification.ProductId);
            return;
        }

        var updated = new ProductReadModel
        {
            Id = existing.Id,
            Name = existing.Name,
            Price = notification.NewPrice,
            CreatedAt = existing.CreatedAt,
            LastUpdatedAt = notification.UpdatedAt
        };

        await readRepository.UpsertAsync(updated, cancellationToken);
    }
}

using CqrsDemo.Application.Abstractions;
using CqrsDemo.Application.Products.ReadModels;
using CqrsDemo.Domain.Products.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CqrsDemo.Application.Products.EventHandlers;

public sealed class ProductCreatedEventHandler(
    IProductReadRepository readRepository,
    ILogger<ProductCreatedEventHandler> logger) : INotificationHandler<ProductCreatedEvent>
{
    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Projecting ProductCreatedEvent for product {ProductId}",
            notification.ProductId);

        var readModel = new ProductReadModel
        {
            Id = notification.ProductId,
            Name = notification.Name,
            Price = notification.Price,
            CreatedAt = notification.CreatedAt,
            LastUpdatedAt = notification.CreatedAt
        };

        await readRepository.UpsertAsync(readModel, cancellationToken);
    }
}

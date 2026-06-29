using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Products;
using CqrsDemo.Contracts.Serialization;
using Inventory.Application.Abstractions;
using Inventory.Application.Commands.InitializeInventory;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Integration;

public sealed class InventoryIntegrationHandlers(IMediator mediator, IInventoryRepository repository, ILogger<InventoryIntegrationHandlers> logger)
{
    public async Task HandleAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        switch (eventType)
        {
            case IntegrationEventTypes.ProductCreated:
                await HandleProductCreatedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.ProductUpdated:
                await HandleProductRenamedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.ProductDeleted:
                await HandleProductDeletedAsync(payload, cancellationToken);
                break;
            default:
                logger.LogDebug("Inventory integration skipped event type {EventType}", eventType);
                break;
        }
    }

    private async Task HandleProductCreatedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<ProductCreatedIntegrationEvent>(payload);
        await mediator.Send(new InitializeInventoryCommand(e.ProductId, e.Name, 100), cancellationToken);
    }

    private async Task HandleProductRenamedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<ProductUpdatedIntegrationEvent>(payload);
        var item = await repository.GetByProductIdAsync(e.ProductId, cancellationToken);
        if (item is null) return;

        item.Rename(e.Name);
        await repository.UpdateAsync(item, cancellationToken);
    }

    private async Task HandleProductDeletedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<ProductDeletedIntegrationEvent>(payload);
        try
        {
            await repository.DeleteByProductIdAsync(e.ProductId, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Skipped inventory delete for product {ProductId}", e.ProductId);
        }
    }
}

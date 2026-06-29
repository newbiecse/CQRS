using Cart.Application.Abstractions;
using Cart.Application.ReadModels;
using CqrsDemo.Contracts.Carts;
using CqrsDemo.Contracts.Common;
using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Serialization;
using Microsoft.Extensions.Logging;

namespace Cart.Infrastructure.Projections;

public sealed class CartProjectionHandler(ICartReadRepository repository, ILogger<CartProjectionHandler> logger)
{
    public async Task HandleAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        switch (eventType)
        {
            case IntegrationEventTypes.CartCreated:
                await ProjectCartCreatedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.CartItemAdded:
                await ProjectCartItemAddedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.CartItemRemoved:
                await ProjectCartItemRemovedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.CartCheckedOut:
                await ProjectCartCheckedOutAsync(payload, cancellationToken);
                break;
            default:
                logger.LogWarning("Unknown integration event type: {EventType}", eventType);
                break;
        }
    }

    private async Task ProjectCartCreatedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<CartCreatedIntegrationEvent>(payload);
        await repository.UpsertAsync(new CartReadModel
        {
            Id = e.CartId,
            CustomerId = e.CustomerId,
            Status = "Active",
            Items = [],
            Subtotal = 0,
            CreatedAt = e.CreatedAt,
            LastUpdatedAt = e.CreatedAt
        }, cancellationToken);
    }

    private async Task ProjectCartItemAddedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<CartItemAddedIntegrationEvent>(payload);
        var cart = await repository.GetByIdAsync(e.CartId, cancellationToken);
        if (cart is null) return;

        var items = cart.Items.ToList();
        var existing = items.FirstOrDefault(i => i.ProductId == e.ProductId);
        if (existing is null)
            items.Add(new OrderLineDto(e.ProductId, e.ProductName, e.UnitPrice, e.Quantity));
        else
        {
            items.Remove(existing);
            items.Add(new OrderLineDto(e.ProductId, e.ProductName, e.UnitPrice, existing.Quantity + e.Quantity));
        }

        await UpsertCartAsync(cart, items, cancellationToken);
    }

    private async Task ProjectCartItemRemovedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<CartItemRemovedIntegrationEvent>(payload);
        var cart = await repository.GetByIdAsync(e.CartId, cancellationToken);
        if (cart is null) return;
        var items = cart.Items.Where(i => i.ProductId != e.ProductId).ToList();
        await UpsertCartAsync(cart, items, cancellationToken);
    }

    private async Task ProjectCartCheckedOutAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<CartCheckedOutIntegrationEvent>(payload);
        var cart = await repository.GetByIdAsync(e.CartId, cancellationToken);
        if (cart is null) return;

        await repository.UpsertAsync(new CartReadModel
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            Status = "CheckedOut",
            Items = cart.Items,
            Subtotal = cart.Subtotal,
            CreatedAt = cart.CreatedAt,
            LastUpdatedAt = e.CheckedOutAt
        }, cancellationToken);
    }

    private async Task UpsertCartAsync(CartReadModel cart, List<OrderLineDto> items, CancellationToken cancellationToken) =>
        await repository.UpsertAsync(new CartReadModel
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            Status = cart.Status,
            Items = items,
            Subtotal = items.Sum(i => i.LineTotal),
            CreatedAt = cart.CreatedAt,
            LastUpdatedAt = DateTime.UtcNow
        }, cancellationToken);
}

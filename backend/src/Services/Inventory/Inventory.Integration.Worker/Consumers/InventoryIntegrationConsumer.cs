using CqrsDemo.Contracts.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Inventory.Application.Integration;

namespace Inventory.Integration.Worker.Consumers;

public sealed class InventoryIntegrationConsumer(IServiceScopeFactory scopeFactory) : IConsumer<IntegrationEventEnvelope>
{
    private static readonly HashSet<string> SupportedEvents =
    [
        IntegrationEventTypes.ProductCreated,
        IntegrationEventTypes.ProductUpdated,
        IntegrationEventTypes.ProductDeleted
    ];

    public async Task Consume(ConsumeContext<IntegrationEventEnvelope> context)
    {
        var message = context.Message;
        if (!SupportedEvents.Contains(message.EventType))
            return;

        using var scope = scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetRequiredService<InventoryIntegrationHandlers>();
        await handlers.HandleAsync(message.EventType, message.Payload, context.CancellationToken);
    }
}

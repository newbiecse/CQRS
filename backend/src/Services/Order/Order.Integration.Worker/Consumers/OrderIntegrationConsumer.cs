using CqrsDemo.Contracts.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Integration;

namespace Order.Integration.Worker.Consumers;

public sealed class OrderIntegrationConsumer(IServiceScopeFactory scopeFactory) : IConsumer<IntegrationEventEnvelope>
{
    public async Task Consume(ConsumeContext<IntegrationEventEnvelope> context)
    {
        var message = context.Message;
        if (message.EventType != IntegrationEventTypes.CartCheckedOut)
            return;

        using var scope = scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetRequiredService<OrderIntegrationHandlers>();
        await handlers.HandleCartCheckedOutAsync(message.Payload, context.CancellationToken);
    }
}

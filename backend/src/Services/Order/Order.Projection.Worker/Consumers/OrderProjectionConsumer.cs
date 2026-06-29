using CqrsDemo.Contracts.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Order.Infrastructure.Projections;

namespace Order.Projection.Worker.Consumers;

public sealed class OrderProjectionConsumer(IServiceScopeFactory scopeFactory) : IConsumer<IntegrationEventEnvelope>
{
    public async Task Consume(ConsumeContext<IntegrationEventEnvelope> context)
    {
        using var scope = scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OrderProjectionHandler>();
        var message = context.Message;
        await handler.HandleAsync(message.EventType, message.Payload, context.CancellationToken);
    }
}

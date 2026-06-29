using CqrsDemo.Contracts.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Product.Infrastructure.Projections;

namespace Product.Projection.Worker.Consumers;

public sealed class ProductProjectionConsumer(IServiceScopeFactory scopeFactory) : IConsumer<IntegrationEventEnvelope>
{
    public async Task Consume(ConsumeContext<IntegrationEventEnvelope> context)
    {
        using var scope = scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ProductProjectionHandler>();
        var message = context.Message;
        await handler.HandleAsync(message.EventType, message.Payload, context.CancellationToken);
    }
}

using CqrsDemo.Contracts.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Payment.Infrastructure.Projections;

namespace Payment.Projection.Worker.Consumers;

public sealed class PaymentProjectionConsumer(IServiceScopeFactory scopeFactory) : IConsumer<IntegrationEventEnvelope>
{
    public async Task Consume(ConsumeContext<IntegrationEventEnvelope> context)
    {
        using var scope = scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<PaymentProjectionHandler>();
        var message = context.Message;
        await handler.HandleAsync(message.EventType, message.Payload, context.CancellationToken);
    }
}

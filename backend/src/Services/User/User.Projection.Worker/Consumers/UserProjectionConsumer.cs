using CqrsDemo.Contracts.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using User.Infrastructure.Projections;

namespace User.Projection.Worker.Consumers;

public sealed class UserProjectionConsumer(IServiceScopeFactory scopeFactory) : IConsumer<IntegrationEventEnvelope>
{
    public async Task Consume(ConsumeContext<IntegrationEventEnvelope> context)
    {
        using var scope = scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<UserProjectionHandler>();
        var message = context.Message;
        await handler.HandleAsync(message.EventType, message.Payload, context.CancellationToken);
    }
}

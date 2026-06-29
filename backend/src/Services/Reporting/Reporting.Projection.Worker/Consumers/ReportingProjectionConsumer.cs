using CqrsDemo.Contracts.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Reporting.Infrastructure.Projections;

namespace Reporting.Projection.Worker.Consumers;

public sealed class ReportingProjectionConsumer(IServiceScopeFactory scopeFactory) : IConsumer<IntegrationEventEnvelope>
{
    public async Task Consume(ConsumeContext<IntegrationEventEnvelope> context)
    {
        using var scope = scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ReportingProjectionHandler>();
        var message = context.Message;
        await handler.HandleAsync(message.EventType, message.Payload, context.CancellationToken);
    }
}

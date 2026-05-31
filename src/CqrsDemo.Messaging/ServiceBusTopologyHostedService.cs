using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CqrsDemo.Messaging;

public sealed class ServiceBusTopologyHostedService(
    ServiceBusTopologyInitializer topologyInitializer,
    ILogger<ServiceBusTopologyHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await topologyInitializer.EnsureCreatedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to ensure Service Bus topology. Verify emulator/Azure Service Bus is running.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

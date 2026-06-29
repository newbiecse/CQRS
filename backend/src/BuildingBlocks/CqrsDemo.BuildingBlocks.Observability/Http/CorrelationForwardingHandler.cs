using CqrsDemo.Contracts.Messaging;

namespace CqrsDemo.BuildingBlocks.Observability.Http;

public sealed class CorrelationForwardingHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = CorrelationContext.CorrelationId ?? CorrelationContext.GetOrCreateCorrelationId();
        if (!request.Headers.Contains(CorrelationHeaders.CorrelationId))
            request.Headers.TryAddWithoutValidation(CorrelationHeaders.CorrelationId, correlationId);

        if (!string.IsNullOrWhiteSpace(CorrelationContext.TraceId)
            && !request.Headers.Contains(CorrelationHeaders.TraceId))
        {
            request.Headers.TryAddWithoutValidation(CorrelationHeaders.TraceId, CorrelationContext.TraceId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}

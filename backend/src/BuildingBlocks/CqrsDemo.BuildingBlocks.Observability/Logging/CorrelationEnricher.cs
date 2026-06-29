using System.Diagnostics;
using CqrsDemo.Contracts.Messaging;
using Serilog.Core;
using Serilog.Events;

namespace CqrsDemo.BuildingBlocks.Observability.Logging;

public sealed class CorrelationEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!string.IsNullOrWhiteSpace(CorrelationContext.CorrelationId))
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("CorrelationId", CorrelationContext.CorrelationId));
        }

        if (!string.IsNullOrWhiteSpace(CorrelationContext.TraceId))
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("TraceId", CorrelationContext.TraceId));
        }

        if (Activity.Current is { } activity)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("SpanId", activity.SpanId.ToString()));
        }
    }
}

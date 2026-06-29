namespace CqrsDemo.BuildingBlocks.Observability.Options;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string ServiceName { get; set; } = "cqrs-demo";
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";
    public string ElasticsearchUrl { get; set; } = "http://localhost:9200";
    public string LogIndexFormat { get; set; } = "app-logs-{0:yyyy.MM.dd}";
    public bool EnableElasticsearchLogSink { get; set; } = true;
    public bool EnableOtlpExporter { get; set; } = true;
}

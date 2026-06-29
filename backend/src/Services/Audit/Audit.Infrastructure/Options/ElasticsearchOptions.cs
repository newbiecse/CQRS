namespace Audit.Infrastructure.Options;

public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public string Url { get; set; } = "http://localhost:9200";
    public string BusinessAuditIndex { get; set; } = "business-audit";
}

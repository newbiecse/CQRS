namespace Product.Infrastructure.Options;

public sealed class ProductElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public string Url { get; set; } = "http://localhost:9200";
    public string ProductIndex { get; set; } = "products";
}

namespace Product.Application.ReadModels;

public sealed class ProductSearchResult
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string HighlightedName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastUpdatedAt { get; init; }
}

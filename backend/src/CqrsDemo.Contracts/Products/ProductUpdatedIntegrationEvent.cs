namespace CqrsDemo.Contracts.Products;

public sealed record ProductUpdatedIntegrationEvent(
    Guid ProductId,
    string Name,
    decimal Price,
    DateTime UpdatedAt);

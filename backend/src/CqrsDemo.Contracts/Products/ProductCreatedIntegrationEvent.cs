namespace CqrsDemo.Contracts.Products;

public sealed record ProductCreatedIntegrationEvent(
    Guid ProductId,
    string Name,
    decimal Price,
    DateTime CreatedAt);

namespace CqrsDemo.Contracts.Products;

public sealed record ProductDeletedIntegrationEvent(
    Guid ProductId,
    DateTime DeletedAt);

namespace CqrsDemo.Contracts.Products;

public sealed record ProductPriceUpdatedIntegrationEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice,
    DateTime UpdatedAt);

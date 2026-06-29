namespace CqrsDemo.Contracts.Carts;

public sealed record CartItemAddedIntegrationEvent(
    Guid CartId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);

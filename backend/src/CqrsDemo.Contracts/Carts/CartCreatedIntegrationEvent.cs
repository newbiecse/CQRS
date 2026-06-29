namespace CqrsDemo.Contracts.Carts;

public sealed record CartCreatedIntegrationEvent(Guid CartId, Guid CustomerId, DateTime CreatedAt);

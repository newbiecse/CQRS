namespace CqrsDemo.Contracts.Carts;

public sealed record CartItemRemovedIntegrationEvent(Guid CartId, Guid ProductId);

namespace CqrsDemo.Contracts.Orders;

public sealed record OrderUpdatedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid CartId,
    IReadOnlyList<Common.OrderLineDto> Lines,
    decimal TotalAmount,
    DateTime UpdatedAt);

using CqrsDemo.Contracts.Common;

namespace CqrsDemo.Contracts.Orders;

public sealed record OrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid CartId,
    IReadOnlyList<OrderLineDto> Lines,
    decimal TotalAmount,
    DateTime CreatedAt);

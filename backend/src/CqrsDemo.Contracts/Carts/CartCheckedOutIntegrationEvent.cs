using CqrsDemo.Contracts.Common;

namespace CqrsDemo.Contracts.Carts;

public sealed record CartCheckedOutIntegrationEvent(
    Guid CartId,
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyList<OrderLineDto> Lines,
    decimal TotalAmount,
    DateTime CheckedOutAt);

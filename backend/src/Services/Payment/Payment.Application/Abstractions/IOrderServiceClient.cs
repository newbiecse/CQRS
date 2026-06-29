namespace Payment.Application.Abstractions;

public interface IOrderServiceClient
{
    Task<OrderSummary?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public sealed record OrderSummary(Guid OrderId, decimal TotalAmount, string Status);

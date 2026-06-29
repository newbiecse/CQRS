namespace CheckoutSaga.Application.Abstractions;

public interface ICartCommandClient
{
    Task<Guid> CheckoutAsync(Guid cartId, CancellationToken cancellationToken);
}

public interface IPaymentCommandClient
{
    Task<Guid> PayOrderAsync(Guid orderId, bool simulateFailure, CancellationToken cancellationToken);
}

public interface IOrderCommandClient
{
    Task MarkOrderPaidAsync(Guid orderId, Guid paymentId, decimal amount, CancellationToken cancellationToken);
    Task CancelOrderAsync(Guid orderId, string reason, CancellationToken cancellationToken);
}

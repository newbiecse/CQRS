using CqrsDemo.Queries.Application.Payments.ReadModels;

namespace CqrsDemo.Queries.Application.Abstractions;

public interface IPaymentReadRepository
{
    Task UpsertAsync(PaymentReadModel payment, CancellationToken cancellationToken = default);
    Task<PaymentReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaymentReadModel?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}

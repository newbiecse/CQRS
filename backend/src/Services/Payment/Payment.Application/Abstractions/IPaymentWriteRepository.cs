using Payment.Domain;

namespace Payment.Application.Abstractions;

public interface IPaymentWriteRepository
{
    Task AddAsync(PaymentAggregate payment, CancellationToken cancellationToken = default);
    Task<PaymentAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(PaymentAggregate payment, CancellationToken cancellationToken = default);
}

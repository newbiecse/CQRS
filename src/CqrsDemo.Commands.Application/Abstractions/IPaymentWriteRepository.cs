using CqrsDemo.Domain.Payments;

namespace CqrsDemo.Commands.Application.Abstractions;

public interface IPaymentWriteRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}

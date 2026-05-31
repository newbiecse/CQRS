using MediatR;
using Payment.Application.Abstractions;
using Payment.Application.ReadModels;

namespace Payment.Application.Queries.GetPaymentByOrderId;

public sealed class GetPaymentByOrderIdQueryHandler(IPaymentReadRepository repository)
    : IRequestHandler<GetPaymentByOrderIdQuery, PaymentReadModel?>
{
    public Task<PaymentReadModel?> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken) =>
        repository.GetByOrderIdAsync(request.OrderId, cancellationToken);
}

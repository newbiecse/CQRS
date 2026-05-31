using CqrsDemo.Queries.Application.Abstractions;
using CqrsDemo.Queries.Application.Payments.ReadModels;
using MediatR;

namespace CqrsDemo.Queries.Application.Payments.Queries.GetPaymentByOrderId;

public sealed class GetPaymentByOrderIdQueryHandler(IPaymentReadRepository repository)
    : IRequestHandler<GetPaymentByOrderIdQuery, PaymentReadModel?>
{
    public Task<PaymentReadModel?> Handle(
        GetPaymentByOrderIdQuery request,
        CancellationToken cancellationToken) =>
        repository.GetByOrderIdAsync(request.OrderId, cancellationToken);
}

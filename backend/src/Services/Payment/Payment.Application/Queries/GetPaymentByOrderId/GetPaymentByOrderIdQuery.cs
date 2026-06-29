using MediatR;
using Payment.Application.ReadModels;

namespace Payment.Application.Queries.GetPaymentByOrderId;

public sealed record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<PaymentReadModel?>;

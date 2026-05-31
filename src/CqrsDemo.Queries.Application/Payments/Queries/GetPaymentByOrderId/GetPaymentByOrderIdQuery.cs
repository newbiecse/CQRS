using CqrsDemo.Queries.Application.Payments.ReadModels;
using MediatR;

namespace CqrsDemo.Queries.Application.Payments.Queries.GetPaymentByOrderId;

public sealed record GetPaymentByOrderIdQuery(Guid OrderId) : IRequest<PaymentReadModel?>;

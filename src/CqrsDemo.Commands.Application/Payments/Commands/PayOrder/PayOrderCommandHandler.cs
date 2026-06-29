using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Domain.Orders;
using CqrsDemo.Domain.Payments;
using MediatR;

namespace CqrsDemo.Commands.Application.Payments.Commands.PayOrder;

public sealed class PayOrderCommandHandler(
    IOrderWriteRepository orderRepository,
    IPaymentWriteRepository paymentRepository)
    : IRequestHandler<PayOrderCommand, Guid>
{
    public async Task<Guid> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        var payment = Payment.Initiate(order.Id, order.TotalAmount);
        payment.Complete();

        await paymentRepository.AddAsync(payment, cancellationToken);

        order.MarkAsPaid(payment.Id, payment.Amount);
        await orderRepository.UpdateAsync(order, cancellationToken);

        return payment.Id;
    }
}

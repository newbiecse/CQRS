using CqrsDemo.Commands.Application.Abstractions;
using CqrsDemo.Domain.Orders;
using CqrsDemo.Domain.Payments;
using MediatR;

namespace CqrsDemo.Commands.Application.Payments.Commands.PayOrder;

public sealed class PayOrderCommandHandler(IEventStore eventStore)
    : IRequestHandler<PayOrderCommand, Guid>
{
    public async Task<Guid> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await eventStore.LoadAsync(
            request.OrderId,
            Order.StreamType,
            Order.LoadFromHistory,
            cancellationToken) ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        var payment = Payment.Initiate(order.Id, order.TotalAmount);
        payment.Complete();

        await eventStore.SaveNewAsync(payment, Payment.StreamType, cancellationToken);

        order.MarkAsPaid(payment.Id, payment.Amount);
        await eventStore.SaveAsync(order, Order.StreamType, cancellationToken);

        return payment.Id;
    }
}

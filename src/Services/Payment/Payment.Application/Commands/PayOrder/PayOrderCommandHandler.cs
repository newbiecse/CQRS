using CqrsDemo.BuildingBlocks.EventStore.Abstractions;
using MediatR;
using Payment.Application.Abstractions;
using Payment.Domain;

namespace Payment.Application.Commands.PayOrder;

public sealed class PayOrderCommandHandler(IEventStore eventStore, IOrderServiceClient orderServiceClient)
    : IRequestHandler<PayOrderCommand, Guid>
{
    public async Task<Guid> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderServiceClient.GetOrderAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order {request.OrderId} was not found.");

        var payment = PaymentAggregate.Initiate(request.OrderId, order.TotalAmount);
        if (request.SimulateFailure)
            payment.Fail("Simulated payment failure for saga compensation demo.");
        else
            payment.Complete();
        await eventStore.SaveNewAsync(payment, PaymentAggregate.StreamType, cancellationToken);
        return payment.Id;
    }
}

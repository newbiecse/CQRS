using MediatR;

namespace Order.Application.Commands.DeleteOrder;

public sealed record DeleteOrderCommand(Guid OrderId, string Reason = "Deleted by admin") : IRequest;

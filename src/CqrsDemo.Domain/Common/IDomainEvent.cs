using MediatR;

namespace CqrsDemo.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}

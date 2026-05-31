using MediatR;

namespace CqrsDemo.BuildingBlocks.Domain;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}

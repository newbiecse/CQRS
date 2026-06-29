using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Orders;
using CqrsDemo.Contracts.Serialization;
using CqrsDemo.Contracts.Users;
using Reporting.Application.Abstractions;

namespace Reporting.Infrastructure.Projections;

public sealed class ReportingProjectionHandler(IReportingWriteRepository repository)
{
    public async Task HandleAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        switch (eventType)
        {
            case IntegrationEventTypes.UserRegistered:
                await ProjectUserRegisteredAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.UserProfileUpdated:
                await ProjectUserProfileUpdatedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.UserDeactivated:
                await ProjectUserDeactivatedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.OrderCreated:
                await ProjectOrderCreatedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.OrderPaid:
                await ProjectOrderPaidAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.OrderCancelled:
                await ProjectOrderCancelledAsync(payload, cancellationToken);
                break;
        }
    }

    private async Task ProjectUserRegisteredAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<UserRegisteredIntegrationEvent>(payload);
        await repository.UpsertUserProfileAsync(
            e.UserId, e.Email, e.DisplayName, isActive: true, e.RegisteredAt, cancellationToken);
    }

    private async Task ProjectUserProfileUpdatedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<UserProfileUpdatedIntegrationEvent>(payload);
        await repository.UpsertUserProfileAsync(
            e.UserId, e.Email, e.DisplayName, isActive: true, e.UpdatedAt, cancellationToken);
    }

    private async Task ProjectUserDeactivatedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<UserDeactivatedIntegrationEvent>(payload);
        await repository.DeactivateUserProfileAsync(e.UserId, e.DeactivatedAt, cancellationToken);
    }

    private async Task ProjectOrderCreatedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<OrderCreatedIntegrationEvent>(payload);
        await repository.UpsertOrderFactAsync(
            e.OrderId,
            e.CustomerId,
            userEmail: null,
            userDisplayName: null,
            e.TotalAmount,
            status: "PendingPayment",
            e.CreatedAt,
            cancellationToken);
    }

    private async Task ProjectOrderPaidAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<OrderPaidIntegrationEvent>(payload);
        await repository.UpdateOrderStatusAsync(e.OrderId, "Paid", e.PaidAt, cancellationToken);
    }

    private async Task ProjectOrderCancelledAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<OrderCancelledIntegrationEvent>(payload);
        await repository.UpdateOrderStatusAsync(e.OrderId, "Cancelled", e.CancelledAt, cancellationToken);
    }
}

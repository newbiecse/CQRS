using CqrsDemo.Contracts.Messaging;
using CqrsDemo.Contracts.Payments;
using CqrsDemo.Contracts.Serialization;
using Microsoft.Extensions.Logging;
using Payment.Application.Abstractions;
using Payment.Application.ReadModels;

namespace Payment.Infrastructure.Projections;

public sealed class PaymentProjectionHandler(IPaymentReadRepository repository, ILogger<PaymentProjectionHandler> logger)
{
    public async Task HandleAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        switch (eventType)
        {
            case IntegrationEventTypes.PaymentInitiated:
                await ProjectPaymentInitiatedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.PaymentCompleted:
                await ProjectPaymentCompletedAsync(payload, cancellationToken);
                break;
            case IntegrationEventTypes.PaymentFailed:
                await ProjectPaymentFailedAsync(payload, cancellationToken);
                break;
            default:
                logger.LogWarning("Unknown integration event type: {EventType}", eventType);
                break;
        }
    }

    private async Task ProjectPaymentInitiatedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<PaymentInitiatedIntegrationEvent>(payload);
        await repository.UpsertAsync(new PaymentReadModel
        {
            Id = e.PaymentId,
            OrderId = e.OrderId,
            Amount = e.Amount,
            Status = "Pending",
            InitiatedAt = e.InitiatedAt,
            LastUpdatedAt = e.InitiatedAt
        }, cancellationToken);
    }

    private async Task ProjectPaymentCompletedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<PaymentCompletedIntegrationEvent>(payload);
        var existing = await repository.GetByIdAsync(e.PaymentId, cancellationToken);
        await repository.UpsertAsync(new PaymentReadModel
        {
            Id = e.PaymentId,
            OrderId = e.OrderId,
            Amount = e.Amount,
            Status = "Completed",
            FailureReason = null,
            InitiatedAt = existing?.InitiatedAt ?? e.CompletedAt,
            LastUpdatedAt = e.CompletedAt
        }, cancellationToken);
    }

    private async Task ProjectPaymentFailedAsync(string payload, CancellationToken cancellationToken)
    {
        var e = IntegrationEventSerializer.Deserialize<PaymentFailedIntegrationEvent>(payload);
        var existing = await repository.GetByIdAsync(e.PaymentId, cancellationToken);
        await repository.UpsertAsync(new PaymentReadModel
        {
            Id = e.PaymentId,
            OrderId = e.OrderId,
            Amount = existing?.Amount ?? 0,
            Status = "Failed",
            FailureReason = e.Reason,
            InitiatedAt = existing?.InitiatedAt ?? e.FailedAt,
            LastUpdatedAt = e.FailedAt
        }, cancellationToken);
    }
}

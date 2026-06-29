using CheckoutSaga.Application.Abstractions;
using CheckoutSaga.Domain;
using CqrsDemo.Contracts.Saga;
using Microsoft.Extensions.Logging;

namespace CheckoutSaga.Application.Orchestration;

public sealed class CheckoutSagaOrchestrator(
    ICheckoutSagaRepository repository,
    ICartCommandClient cartClient,
    IPaymentCommandClient paymentClient,
    IOrderCommandClient orderClient,
    ICheckoutSagaNotifier notifier,
    ILogger<CheckoutSagaOrchestrator> logger)
{
    public async Task<CheckoutSagaInstance> StartAsync(Guid cartId, bool simulatePaymentFailure, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByCartIdAsync(cartId, cancellationToken);
        if (existing is not null && !existing.IsTerminal)
            throw new InvalidOperationException($"Checkout saga already in progress for cart {cartId}.");

        var saga = new CheckoutSagaInstance
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            SimulatePaymentFailure = simulatePaymentFailure,
            State = CheckoutSagaStates.Started,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await repository.AddAsync(saga, cancellationToken);

        try
        {
            var orderId = await cartClient.CheckoutAsync(cartId, cancellationToken);
            saga.OrderId = orderId;
            saga.State = CheckoutSagaStates.CartCheckedOut;
            saga.UpdatedAt = DateTime.UtcNow;
            await repository.UpdateAsync(saga, cancellationToken);
            return saga;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Checkout saga {SagaId} failed during cart checkout", saga.Id);
            saga.State = CheckoutSagaStates.Failed;
            saga.FailureReason = ex.Message;
            saga.UpdatedAt = DateTime.UtcNow;
            await repository.UpdateAsync(saga, cancellationToken);
            await notifier.PublishFailedAsync(saga, cancellationToken);
            throw;
        }
    }

    public async Task HandleOrderCreatedAsync(Guid orderId, Guid cartId, CancellationToken cancellationToken)
    {
        var saga = await repository.GetByOrderIdAsync(orderId, cancellationToken)
            ?? await repository.GetByCartIdAsync(cartId, cancellationToken);
        if (saga is null || saga.IsTerminal) return;

        if (saga.OrderId is null)
        {
            saga.OrderId = orderId;
            saga.UpdatedAt = DateTime.UtcNow;
            await repository.UpdateAsync(saga, cancellationToken);
        }

        if (saga.State is CheckoutSagaStates.Started or CheckoutSagaStates.CartCheckedOut)
        {
            saga.State = CheckoutSagaStates.OrderCreated;
            saga.UpdatedAt = DateTime.UtcNow;
            await repository.UpdateAsync(saga, cancellationToken);
        }

        await TryInitiatePaymentAsync(saga, cancellationToken);
    }

    public async Task HandlePaymentCompletedAsync(
        Guid paymentId,
        Guid orderId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var saga = await repository.GetByOrderIdAsync(orderId, cancellationToken);
        if (saga is null || saga.IsTerminal) return;
        if (saga.State is not CheckoutSagaStates.PaymentPending and not CheckoutSagaStates.OrderCreated)
            return;

        try
        {
            await orderClient.MarkOrderPaidAsync(orderId, paymentId, amount, cancellationToken);
            saga.PaymentId = paymentId;
            saga.State = CheckoutSagaStates.Completed;
            saga.UpdatedAt = DateTime.UtcNow;
            await repository.UpdateAsync(saga, cancellationToken);
            await notifier.PublishCompletedAsync(saga, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Checkout saga {SagaId} failed marking order {OrderId} as paid", saga.Id, orderId);
            await FailSagaAsync(saga, ex.Message, cancellationToken);
        }
    }

    public async Task HandlePaymentFailedAsync(Guid orderId, string reason, CancellationToken cancellationToken)
    {
        var saga = await repository.GetByOrderIdAsync(orderId, cancellationToken);
        if (saga is null || saga.IsTerminal) return;

        saga.State = CheckoutSagaStates.PaymentFailed;
        saga.FailureReason = reason;
        saga.UpdatedAt = DateTime.UtcNow;
        await repository.UpdateAsync(saga, cancellationToken);
        await CompensateAsync(saga, reason, cancellationToken);
    }

    private async Task TryInitiatePaymentAsync(CheckoutSagaInstance saga, CancellationToken cancellationToken)
    {
        if (saga.OrderId is null) return;
        if (saga.State is not CheckoutSagaStates.OrderCreated and not CheckoutSagaStates.CartCheckedOut) return;
        if (saga.State == CheckoutSagaStates.PaymentPending) return;

        saga.State = CheckoutSagaStates.PaymentPending;
        saga.UpdatedAt = DateTime.UtcNow;
        await repository.UpdateAsync(saga, cancellationToken);

        try
        {
            var paymentId = await paymentClient.PayOrderAsync(
                saga.OrderId.Value,
                saga.SimulatePaymentFailure,
                cancellationToken);
            saga.PaymentId = paymentId;
            saga.UpdatedAt = DateTime.UtcNow;
            await repository.UpdateAsync(saga, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Checkout saga {SagaId} failed initiating payment for order {OrderId}", saga.Id, saga.OrderId);
            await FailSagaAsync(saga, ex.Message, cancellationToken);
        }
    }

    private async Task CompensateAsync(CheckoutSagaInstance saga, string reason, CancellationToken cancellationToken)
    {
        if (saga.OrderId is null)
        {
            await FailSagaAsync(saga, reason, cancellationToken);
            return;
        }

        saga.State = CheckoutSagaStates.Compensating;
        saga.UpdatedAt = DateTime.UtcNow;
        await repository.UpdateAsync(saga, cancellationToken);

        try
        {
            await orderClient.CancelOrderAsync(saga.OrderId.Value, $"Saga compensation: {reason}", cancellationToken);
            saga.State = CheckoutSagaStates.Compensated;
            saga.UpdatedAt = DateTime.UtcNow;
            await repository.UpdateAsync(saga, cancellationToken);
            await notifier.PublishFailedAsync(saga, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Checkout saga {SagaId} compensation failed for order {OrderId}", saga.Id, saga.OrderId);
            await FailSagaAsync(saga, $"Compensation failed: {ex.Message}", cancellationToken);
        }
    }

    private async Task FailSagaAsync(CheckoutSagaInstance saga, string reason, CancellationToken cancellationToken)
    {
        saga.State = CheckoutSagaStates.Failed;
        saga.FailureReason = reason;
        saga.UpdatedAt = DateTime.UtcNow;
        await repository.UpdateAsync(saga, cancellationToken);
        await notifier.PublishFailedAsync(saga, cancellationToken);
    }
}

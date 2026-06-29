using CheckoutSaga.Domain;

namespace CheckoutSaga.Application.Abstractions;

public interface ICheckoutSagaNotifier
{
    Task PublishCompletedAsync(CheckoutSagaInstance saga, CancellationToken cancellationToken);
    Task PublishFailedAsync(CheckoutSagaInstance saga, CancellationToken cancellationToken);
}

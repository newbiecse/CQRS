using CheckoutSaga.Application.Abstractions;
using CheckoutSaga.Domain;
using MediatR;

namespace CheckoutSaga.Application.Queries.GetCheckoutSaga;

public sealed class GetCheckoutSagaQueryHandler(ICheckoutSagaRepository repository)
    : IRequestHandler<GetCheckoutSagaQuery, CheckoutSagaInstance?>
{
    public Task<CheckoutSagaInstance?> Handle(GetCheckoutSagaQuery request, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(request.SagaId, cancellationToken);
}

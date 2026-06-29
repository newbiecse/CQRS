using CheckoutSaga.Domain;
using MediatR;

namespace CheckoutSaga.Application.Queries.GetCheckoutSaga;

public sealed record GetCheckoutSagaQuery(Guid SagaId) : IRequest<CheckoutSagaInstance?>;

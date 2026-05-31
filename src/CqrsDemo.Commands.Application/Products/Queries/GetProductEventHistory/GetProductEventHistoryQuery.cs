using CqrsDemo.Commands.Application.Abstractions;
using MediatR;

namespace CqrsDemo.Commands.Application.Products.Queries.GetProductEventHistory;

public sealed record GetProductEventHistoryQuery(Guid ProductId) : IRequest<IReadOnlyList<StoredEventDto>?>;

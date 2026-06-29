using MediatR;
using User.Application.ReadModels;

namespace User.Application.Queries.GetUsersByIds;

public sealed record GetUsersByIdsQuery(IReadOnlyList<Guid> UserIds) : IRequest<IReadOnlyList<UserReadModel>>;

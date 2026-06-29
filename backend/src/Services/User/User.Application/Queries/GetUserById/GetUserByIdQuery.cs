using MediatR;
using User.Application.ReadModels;

namespace User.Application.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserReadModel?>;

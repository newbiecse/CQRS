using MediatR;
using User.Application.ReadModels;

namespace User.Application.Queries.GetAllUsers;

public sealed record GetAllUsersQuery : IRequest<IReadOnlyList<UserReadModel>>;

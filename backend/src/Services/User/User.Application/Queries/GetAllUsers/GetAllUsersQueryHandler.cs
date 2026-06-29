using MediatR;
using User.Application.Abstractions;
using User.Application.ReadModels;

namespace User.Application.Queries.GetAllUsers;

public sealed class GetAllUsersQueryHandler(IUserReadRepository repo)
    : IRequestHandler<GetAllUsersQuery, IReadOnlyList<UserReadModel>>
{
    public Task<IReadOnlyList<UserReadModel>> Handle(GetAllUsersQuery request, CancellationToken ct) =>
        repo.GetAllAsync(ct);
}

using MediatR;
using User.Application.Abstractions;
using User.Application.ReadModels;

namespace User.Application.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(IUserReadRepository repo)
    : IRequestHandler<GetUserByIdQuery, UserReadModel?>
{
    public Task<UserReadModel?> Handle(GetUserByIdQuery request, CancellationToken ct) =>
        repo.GetByIdAsync(request.UserId, ct);
}

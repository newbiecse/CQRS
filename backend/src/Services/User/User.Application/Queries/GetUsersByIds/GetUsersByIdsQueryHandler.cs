using MediatR;
using User.Application.Abstractions;
using User.Application.ReadModels;

namespace User.Application.Queries.GetUsersByIds;

public sealed class GetUsersByIdsQueryHandler(IUserReadRepository repository)
    : IRequestHandler<GetUsersByIdsQuery, IReadOnlyList<UserReadModel>>
{
    public async Task<IReadOnlyList<UserReadModel>> Handle(GetUsersByIdsQuery request, CancellationToken cancellationToken)
    {
        if (request.UserIds.Count == 0)
            return [];

        return await repository.GetByIdsAsync(request.UserIds, cancellationToken);
    }
}

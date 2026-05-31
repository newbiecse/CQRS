using User.Application.ReadModels;

namespace User.Application.Abstractions;

public interface IUserReadRepository
{
    Task UpsertAsync(UserReadModel user, CancellationToken ct = default);
    Task<UserReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserReadModel?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<UserReadModel>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyList<UserReadModel>> GetAllAsync(CancellationToken ct = default);
}

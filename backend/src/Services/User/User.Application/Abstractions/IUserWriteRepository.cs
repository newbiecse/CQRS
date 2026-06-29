using User.Domain;

namespace User.Application.Abstractions;

public interface IUserWriteRepository
{
    Task AddAsync(UserAggregate user, CancellationToken cancellationToken = default);
    Task<UserAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserAggregate user, CancellationToken cancellationToken = default);
}

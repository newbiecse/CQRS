using Audit.Application.Models;

namespace Audit.Application.Abstractions;

public interface IBusinessAuditReader
{
    Task<IReadOnlyList<BusinessAuditRecord>> SearchAsync(
        BusinessAuditSearchQuery query,
        CancellationToken cancellationToken = default);
}

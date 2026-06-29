using Audit.Application.Models;

namespace Audit.Application.Abstractions;

public interface IBusinessAuditWriter
{
    Task IndexAsync(BusinessAuditRecord record, CancellationToken cancellationToken = default);
}

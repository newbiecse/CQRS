using Reporting.Application.ReadModels;

namespace Reporting.Application.Abstractions;

public interface IReportingReadRepository
{
    Task<IReadOnlyList<TopUserByOrderAmountReadModel>> GetTopUsersByOrderAmountAsync(
        ReportingPeriod period,
        int limit,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopProductBySalesReadModel>> GetTopProductsBySalesAsync(
        ReportingPeriod period,
        int limit,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        CancellationToken cancellationToken = default);
}

using MediatR;
using Reporting.Application.Abstractions;
using Reporting.Application.ReadModels;

namespace Reporting.Application.Queries.GetTopProductsBySales;

public sealed class GetTopProductsBySalesQueryHandler(IReportingReadRepository repository)
    : IRequestHandler<GetTopProductsBySalesQuery, IReadOnlyList<TopProductBySalesReadModel>>
{
    public async Task<IReadOnlyList<TopProductBySalesReadModel>> Handle(
        GetTopProductsBySalesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Limit is < 1 or > 100)
            throw new ArgumentException("Limit must be between 1 and 100.", nameof(request.Limit));

        var (periodStart, periodEnd) = ReportingPeriodRange.Resolve(request.Period, request.ReferenceDateUtc);

        return await repository.GetTopProductsBySalesAsync(
            request.Period,
            request.Limit,
            periodStart,
            periodEnd,
            cancellationToken);
    }
}

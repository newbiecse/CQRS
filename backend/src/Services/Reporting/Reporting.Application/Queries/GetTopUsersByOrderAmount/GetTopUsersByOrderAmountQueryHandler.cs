using MediatR;
using Reporting.Application.Abstractions;
using Reporting.Application.ReadModels;

namespace Reporting.Application.Queries.GetTopUsersByOrderAmount;

public sealed class GetTopUsersByOrderAmountQueryHandler(IReportingReadRepository repository)
    : IRequestHandler<GetTopUsersByOrderAmountQuery, IReadOnlyList<TopUserByOrderAmountReadModel>>
{
    public async Task<IReadOnlyList<TopUserByOrderAmountReadModel>> Handle(
        GetTopUsersByOrderAmountQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Limit is < 1 or > 100)
            throw new ArgumentException("Limit must be between 1 and 100.", nameof(request.Limit));

        var (periodStart, periodEnd) = ReportingPeriodRange.Resolve(request.Period, request.ReferenceDateUtc);

        return await repository.GetTopUsersByOrderAmountAsync(
            request.Period,
            request.Limit,
            periodStart,
            periodEnd,
            cancellationToken);
    }
}

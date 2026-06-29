using MediatR;
using Reporting.Application.ReadModels;

namespace Reporting.Application.Queries.GetTopUsersByOrderAmount;

public sealed record GetTopUsersByOrderAmountQuery(
    ReportingPeriod Period,
    int Limit = 10,
    DateTime? ReferenceDateUtc = null) : IRequest<IReadOnlyList<TopUserByOrderAmountReadModel>>;

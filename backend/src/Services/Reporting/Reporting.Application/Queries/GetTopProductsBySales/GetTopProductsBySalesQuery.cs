using MediatR;
using Reporting.Application.ReadModels;

namespace Reporting.Application.Queries.GetTopProductsBySales;

public sealed record GetTopProductsBySalesQuery(
    ReportingPeriod Period,
    int Limit = 10,
    DateTime? ReferenceDateUtc = null) : IRequest<IReadOnlyList<TopProductBySalesReadModel>>;

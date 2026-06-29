using Microsoft.EntityFrameworkCore;
using Reporting.Application;
using Reporting.Application.Abstractions;
using Reporting.Application.ReadModels;

namespace Reporting.Infrastructure.Persistence;

public sealed class SqlReportingReadRepository(ReportingDbContext db) : IReportingReadRepository
{
    public async Task<IReadOnlyList<TopUserByOrderAmountReadModel>> GetTopUsersByOrderAmountAsync(
        ReportingPeriod period,
        int limit,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        CancellationToken cancellationToken = default)
    {
        var rankings = await db.OrderFacts.AsNoTracking()
            .Where(o => o.Status != "Cancelled" && o.OrderCreatedAt >= periodStartUtc && o.OrderCreatedAt < periodEndUtc)
            .GroupBy(o => o.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalOrderAmount = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count(),
                Email = g.Max(o => o.UserEmail),
                DisplayName = g.Max(o => o.UserDisplayName)
            })
            .OrderByDescending(x => x.TotalOrderAmount)
            .ThenByDescending(x => x.OrderCount)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return rankings.Select(x => new TopUserByOrderAmountReadModel
        {
            UserId = x.UserId,
            Email = x.Email ?? string.Empty,
            DisplayName = x.DisplayName ?? string.Empty,
            TotalOrderAmount = x.TotalOrderAmount,
            OrderCount = x.OrderCount,
            Period = period,
            PeriodStartUtc = periodStartUtc,
            PeriodEndUtc = periodEndUtc
        }).ToList();
    }

    public async Task<IReadOnlyList<TopProductBySalesReadModel>> GetTopProductsBySalesAsync(
        ReportingPeriod period,
        int limit,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        CancellationToken cancellationToken = default)
    {
        var rankings = await db.OrderLineFacts.AsNoTracking()
            .Join(
                db.OrderFacts.AsNoTracking(),
                line => line.OrderId,
                order => order.OrderId,
                (line, order) => new { line, order })
            .Where(x => x.order.Status != "Cancelled"
                && x.line.OrderCreatedAt >= periodStartUtc
                && x.line.OrderCreatedAt < periodEndUtc)
            .GroupBy(x => new { x.line.ProductId, x.line.ProductName })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                TotalQuantity = g.Sum(x => x.line.Quantity),
                TotalSalesAmount = g.Sum(x => x.line.LineTotal),
                OrderCount = g.Select(x => x.line.OrderId).Distinct().Count()
            })
            .OrderByDescending(x => x.TotalSalesAmount)
            .ThenByDescending(x => x.TotalQuantity)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return rankings.Select(x => new TopProductBySalesReadModel
        {
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            TotalQuantity = x.TotalQuantity,
            TotalSalesAmount = x.TotalSalesAmount,
            OrderCount = x.OrderCount,
            Period = period,
            PeriodStartUtc = periodStartUtc,
            PeriodEndUtc = periodEndUtc
        }).ToList();
    }
}

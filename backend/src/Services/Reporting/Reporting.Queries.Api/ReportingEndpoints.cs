using MediatR;
using Reporting.Application;
using Reporting.Application.Queries.GetTopProductsBySales;
using Reporting.Application.Queries.GetTopUsersByOrderAmount;

namespace Reporting.Queries.Api;

internal static class ReportingEndpoints
{
    public static void MapReportingEndpoints(this WebApplication app)
    {
        MapTopUsers(app);
        MapTopProducts(app);
    }

    private static void MapTopUsers(WebApplication app)
    {
        app.MapGet("/api/reports/top-users/day", (int? limit, DateTime? date, IMediator mediator) =>
            GetTopUsers(ReportingPeriod.Day, limit, date, mediator));

        app.MapGet("/api/reports/top-users/week", (int? limit, DateTime? date, IMediator mediator) =>
            GetTopUsers(ReportingPeriod.Week, limit, date, mediator));

        app.MapGet("/api/reports/top-users/month", (int? limit, DateTime? date, IMediator mediator) =>
            GetTopUsers(ReportingPeriod.Month, limit, date, mediator));

        app.MapGet("/api/reports/top-users/year", (int? limit, DateTime? date, IMediator mediator) =>
            GetTopUsers(ReportingPeriod.Year, limit, date, mediator));

        app.MapGet("/api/reports/top-users", async (string period, int? limit, DateTime? date, IMediator mediator) =>
        {
            if (!TryParsePeriod(period, out var parsed))
                return Results.BadRequest(new { message = "Query 'period' is required and must be day, week, month, or year." });
            return await GetTopUsers(parsed, limit, date, mediator);
        });
    }

    private static void MapTopProducts(WebApplication app)
    {
        app.MapGet("/api/reports/top-products/day", (int? limit, DateTime? date, IMediator mediator) =>
            GetTopProducts(ReportingPeriod.Day, limit, date, mediator));

        app.MapGet("/api/reports/top-products/week", (int? limit, DateTime? date, IMediator mediator) =>
            GetTopProducts(ReportingPeriod.Week, limit, date, mediator));

        app.MapGet("/api/reports/top-products/month", (int? limit, DateTime? date, IMediator mediator) =>
            GetTopProducts(ReportingPeriod.Month, limit, date, mediator));

        app.MapGet("/api/reports/top-products/year", (int? limit, DateTime? date, IMediator mediator) =>
            GetTopProducts(ReportingPeriod.Year, limit, date, mediator));

        app.MapGet("/api/reports/top-products", async (string period, int? limit, DateTime? date, IMediator mediator) =>
        {
            if (!TryParsePeriod(period, out var parsed))
                return Results.BadRequest(new { message = "Query 'period' is required and must be day, week, month, or year." });
            return await GetTopProducts(parsed, limit, date, mediator);
        });
    }

    private static async Task<IResult> GetTopUsers(
        ReportingPeriod period,
        int? limit,
        DateTime? date,
        IMediator mediator)
    {
        try
        {
            var result = await mediator.Send(new GetTopUsersByOrderAmountQuery(
                period,
                limit ?? 10,
                date));
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> GetTopProducts(
        ReportingPeriod period,
        int? limit,
        DateTime? date,
        IMediator mediator)
    {
        try
        {
            var result = await mediator.Send(new GetTopProductsBySalesQuery(
                period,
                limit ?? 10,
                date));
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static bool TryParsePeriod(string? period, out ReportingPeriod parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(period)) return false;

        return period.Trim().ToLowerInvariant() switch
        {
            "day" or "daily" => Assign(ReportingPeriod.Day, out parsed),
            "week" or "weekly" => Assign(ReportingPeriod.Week, out parsed),
            "month" or "monthly" => Assign(ReportingPeriod.Month, out parsed),
            "year" or "yearly" => Assign(ReportingPeriod.Year, out parsed),
            _ => false
        };
    }

    private static bool Assign(ReportingPeriod value, out ReportingPeriod parsed)
    {
        parsed = value;
        return true;
    }
}

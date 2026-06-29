namespace Reporting.Application;

public static class ReportingPeriodRange
{
    public static (DateTime PeriodStartUtc, DateTime PeriodEndUtc) Resolve(ReportingPeriod period, DateTime? referenceUtc = null)
    {
        var reference = referenceUtc ?? DateTime.UtcNow;
        if (reference.Kind != DateTimeKind.Utc)
            reference = DateTime.SpecifyKind(reference, DateTimeKind.Utc);

        return period switch
        {
            ReportingPeriod.Day => ForDay(reference),
            ReportingPeriod.Week => ForWeek(reference),
            ReportingPeriod.Month => ForMonth(reference),
            _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
        };
    }

    private static (DateTime, DateTime) ForDay(DateTime referenceUtc)
    {
        var start = referenceUtc.Date;
        return (start, start.AddDays(1));
    }

    private static (DateTime, DateTime) ForWeek(DateTime referenceUtc)
    {
        var date = referenceUtc.Date;
        var daysFromMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var start = date.AddDays(-daysFromMonday);
        return (start, start.AddDays(7));
    }

    private static (DateTime, DateTime) ForMonth(DateTime referenceUtc)
    {
        var start = new DateTime(referenceUtc.Year, referenceUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return (start, start.AddMonths(1));
    }
}

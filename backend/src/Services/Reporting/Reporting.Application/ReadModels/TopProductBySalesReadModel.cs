namespace Reporting.Application.ReadModels;

public sealed class TopProductBySalesReadModel
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int TotalQuantity { get; init; }
    public decimal TotalSalesAmount { get; init; }
    public int OrderCount { get; init; }
    public ReportingPeriod Period { get; init; }
    public DateTime PeriodStartUtc { get; init; }
    public DateTime PeriodEndUtc { get; init; }
}

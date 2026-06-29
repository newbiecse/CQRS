namespace Reporting.Application.ReadModels;

public sealed class TopUserByOrderAmountReadModel
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public decimal TotalOrderAmount { get; init; }
    public int OrderCount { get; init; }
    public ReportingPeriod Period { get; init; }
    public DateTime PeriodStartUtc { get; init; }
    public DateTime PeriodEndUtc { get; init; }
}

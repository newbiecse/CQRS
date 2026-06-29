namespace CqrsDemo.BuildingBlocks.RateLimiting;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    public bool Enabled { get; set; } = true;
    public int PermitLimit { get; set; } = 120;
    public int WindowSeconds { get; set; } = 60;
    public int AuthPermitLimit { get; set; } = 15;
    public int ChatPermitLimit { get; set; } = 30;
    public int WritePermitLimit { get; set; } = 60;
}

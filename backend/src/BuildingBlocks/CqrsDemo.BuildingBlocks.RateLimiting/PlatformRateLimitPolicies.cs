namespace CqrsDemo.BuildingBlocks.RateLimiting;

public static class PlatformRateLimitPolicies
{
    public const string Global = "global";
    public const string Auth = "auth";
    public const string Chat = "chat";
    public const string Write = "write";
}

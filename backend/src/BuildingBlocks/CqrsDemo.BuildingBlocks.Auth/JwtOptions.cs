namespace CqrsDemo.BuildingBlocks.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "cqrs-demo";
    public string Audience { get; set; } = "cqrs-demo-clients";
    public string SigningKey { get; set; } = "CqrsDemo-Dev-Signing-Key-Change-In-Production-32chars!";
    public int ExpirationMinutes { get; set; } = 480;
}

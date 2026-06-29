namespace Auth.Infrastructure.Options;

public sealed class OAuthOptions
{
    public const string SectionName = "OAuth";

    public string FrontendCallbackUrl { get; set; } = "http://localhost:8000/user/login";
    public GoogleOAuthOptions Google { get; set; } = new();
    public FacebookOAuthOptions Facebook { get; set; } = new();
}

public sealed class GoogleOAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public sealed class FacebookOAuthOptions
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
}

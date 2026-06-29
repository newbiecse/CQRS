namespace Chat.Application.Options;

public sealed class ChatAgentOptions
{
    public const string SectionName = "ChatAgent";

    public string ProductQueriesBaseUrl { get; set; } = "http://localhost:5211";
    public string? OpenAiApiKey { get; set; }
    public string OpenAiEndpoint { get; set; } = "https://api.openai.com/v1";
    public string Model { get; set; } = "gpt-4o-mini";
    public string AgentName { get; set; } = "CQRS Shop Assistant";
}

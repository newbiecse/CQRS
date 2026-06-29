namespace Auth.Application.ReadModels;

public sealed class AuthenticatedUserReadModel
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}

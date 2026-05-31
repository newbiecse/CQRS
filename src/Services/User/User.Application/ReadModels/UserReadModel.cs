namespace User.Application.ReadModels;

public sealed class UserReadModel
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime RegisteredAt { get; init; }
    public DateTime LastUpdatedAt { get; init; }
}

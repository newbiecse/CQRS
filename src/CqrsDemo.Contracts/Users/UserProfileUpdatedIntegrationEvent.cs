namespace CqrsDemo.Contracts.Users;

public sealed record UserProfileUpdatedIntegrationEvent(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTime UpdatedAt);

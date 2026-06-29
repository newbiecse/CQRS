namespace CqrsDemo.Contracts.Users;

public sealed record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTime RegisteredAt);

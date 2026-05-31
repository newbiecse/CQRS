namespace CqrsDemo.Contracts.Users;

public sealed record UserDeactivatedIntegrationEvent(
    Guid UserId,
    DateTime DeactivatedAt);

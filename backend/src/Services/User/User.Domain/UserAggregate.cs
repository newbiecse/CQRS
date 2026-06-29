using CqrsDemo.BuildingBlocks.Domain;
using User.Domain.Events;

namespace User.Domain;

public sealed class UserAggregate : AggregateRoot
{
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    private UserAggregate() { }

    public static UserAggregate Register(string email, string displayName)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (!email.Contains('@')) throw new ArgumentException("Email format is invalid.", nameof(email));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name is required.", nameof(displayName));

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var trimmedName = displayName.Trim();
        var user = new UserAggregate
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            DisplayName = trimmedName,
            IsActive = true,
            RegisteredAt = DateTime.UtcNow
        };
        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, normalizedEmail, trimmedName, user.RegisteredAt));
        return user;
    }

    public static UserAggregate Restore(
        Guid id,
        string email,
        string displayName,
        bool isActive,
        DateTime registeredAt) =>
        new()
        {
            Id = id,
            Email = email,
            DisplayName = displayName,
            IsActive = isActive,
            RegisteredAt = registeredAt
        };

    public void UpdateProfile(string displayName)
    {
        EnsureActive();
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name is required.", nameof(displayName));

        var trimmed = displayName.Trim();
        if (trimmed == DisplayName) return;

        DisplayName = trimmed;
        RaiseDomainEvent(new UserProfileUpdatedEvent(Id, Email, trimmed, DateTime.UtcNow));
    }

    public void Deactivate()
    {
        if (!IsActive) throw new InvalidOperationException("User is already deactivated.");

        IsActive = false;
        RaiseDomainEvent(new UserDeactivatedEvent(Id, DateTime.UtcNow));
    }

    private void EnsureActive()
    {
        if (!IsActive) throw new InvalidOperationException("User is deactivated.");
    }
}

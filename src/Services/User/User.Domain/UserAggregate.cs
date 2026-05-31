using CqrsDemo.BuildingBlocks.Domain;
using User.Domain.Events;

namespace User.Domain;

public sealed class UserAggregate : AggregateRoot
{
    public const string StreamType = "User";

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

        var user = new UserAggregate();
        user.Raise(new UserRegisteredEvent(
            Guid.NewGuid(),
            email.Trim().ToLowerInvariant(),
            displayName.Trim(),
            DateTime.UtcNow));
        return user;
    }

    public static UserAggregate Load(IReadOnlyList<IDomainEvent> history)
    {
        var user = new UserAggregate();
        foreach (var e in history) user.Apply(e);
        user.SetVersion(history.Count);
        user.ClearDomainEvents();
        return user;
    }

    public void UpdateProfile(string displayName)
    {
        EnsureActive();
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name is required.", nameof(displayName));
        var trimmed = displayName.Trim();
        if (trimmed == DisplayName) return;
        Raise(new UserProfileUpdatedEvent(Id, Email, trimmed, DateTime.UtcNow));
    }

    public void Deactivate()
    {
        if (!IsActive) throw new InvalidOperationException("User is already deactivated.");
        Raise(new UserDeactivatedEvent(Id, DateTime.UtcNow));
    }

    private void EnsureActive()
    {
        if (!IsActive) throw new InvalidOperationException("User is deactivated.");
    }

    private void Raise(IDomainEvent e)
    {
        Apply(e);
        RaiseDomainEvent(e);
    }

    private void Apply(IDomainEvent e)
    {
        switch (e)
        {
            case UserRegisteredEvent registered:
                ApplyRegistered(registered);
                break;
            case UserProfileUpdatedEvent updated:
                DisplayName = updated.DisplayName;
                break;
            case UserDeactivatedEvent:
                IsActive = false;
                break;
            default:
                throw new NotSupportedException(e.GetType().Name);
        }
    }

    private void ApplyRegistered(UserRegisteredEvent registered)
    {
        Id = registered.UserId;
        Email = registered.Email;
        DisplayName = registered.DisplayName;
        RegisteredAt = registered.RegisteredAt;
        IsActive = true;
    }
}

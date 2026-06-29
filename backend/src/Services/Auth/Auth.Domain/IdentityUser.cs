namespace Auth.Domain;

public static class AuthProviders
{
    public const string Local = "local";
    public const string Google = "google";
    public const string Facebook = "facebook";
}

public sealed class IdentityUser
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string RolesCsv { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<string> Roles =>
        string.IsNullOrWhiteSpace(RolesCsv)
            ? Array.Empty<string>()
            : RolesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private IdentityUser() { }

    public static IdentityUser CreateLocal(
        Guid id,
        string email,
        string displayName,
        IEnumerable<string>? roles = null) =>
        new()
        {
            Id = id,
            Email = NormalizeEmail(email),
            DisplayName = NormalizeName(displayName),
            RolesCsv = NormalizeRoles(roles),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    public static IdentityUser CreateFromExternal(
        Guid id,
        string email,
        string displayName,
        IEnumerable<string>? roles = null) =>
        CreateLocal(id, email, displayName, roles);

    public static IdentityUser Restore(
        Guid id,
        string email,
        string displayName,
        string rolesCsv,
        bool isActive,
        DateTime createdAt) =>
        new()
        {
            Id = id,
            Email = email,
            DisplayName = displayName,
            RolesCsv = rolesCsv,
            IsActive = isActive,
            CreatedAt = createdAt
        };

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public void UpdateDisplayName(string displayName)
    {
        DisplayName = NormalizeName(displayName);
    }

    public void UpdateRoles(IEnumerable<string>? roles)
    {
        RolesCsv = NormalizeRoles(roles);
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Email format is invalid.", nameof(email));
        return email.Trim().ToLowerInvariant();
    }

    private static string NormalizeName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required.", nameof(displayName));
        return displayName.Trim();
    }

    private static string NormalizeRoles(IEnumerable<string>? roles)
    {
        var normalized = (roles ?? ["user"])
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return string.Join(',', normalized);
    }
}

namespace Auth.Domain;

public sealed class PermissionDefinition
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private PermissionDefinition() { }

    public static PermissionDefinition Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name is required.", nameof(name));

        return new PermissionDefinition
        {
            Id = Guid.NewGuid(),
            Name = NormalizeName(name),
            Description = description?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static PermissionDefinition Restore(Guid id, string name, string description, DateTime createdAt) =>
        new()
        {
            Id = id,
            Name = name,
            Description = description,
            CreatedAt = createdAt
        };

    public void Update(string description) => Description = description?.Trim() ?? string.Empty;

    private static string NormalizeName(string name) => name.Trim().ToLowerInvariant();
}

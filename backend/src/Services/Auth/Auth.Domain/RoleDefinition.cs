namespace Auth.Domain;

public sealed class RoleDefinition
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsSystem { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private RoleDefinition() { }

    public static RoleDefinition Create(string name, string description, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required.", nameof(name));

        return new RoleDefinition
        {
            Id = Guid.NewGuid(),
            Name = NormalizeName(name),
            Description = description?.Trim() ?? string.Empty,
            IsSystem = isSystem,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static RoleDefinition Restore(Guid id, string name, string description, bool isSystem, DateTime createdAt) =>
        new()
        {
            Id = id,
            Name = name,
            Description = description,
            IsSystem = isSystem,
            CreatedAt = createdAt
        };

    public void Update(string description) => Description = description?.Trim() ?? string.Empty;

    private static string NormalizeName(string name) => name.Trim().ToLowerInvariant();
}

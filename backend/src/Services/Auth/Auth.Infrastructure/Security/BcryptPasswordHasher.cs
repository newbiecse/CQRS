using Auth.Application.Abstractions;

namespace Auth.Infrastructure.Security;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string passwordHash, string password) =>
        BCrypt.Net.BCrypt.Verify(password, passwordHash);
}

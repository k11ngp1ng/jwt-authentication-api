using BCrypt.Net;
using JwtAuthenticationApi.Application.Interfaces;

namespace JwtAuthenticationApi.Infrastructure.Authentication;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return BCrypt.Net.BCrypt.EnhancedHashPassword(
            password,
            HashType.SHA384,
            WorkFactor);
    }

    public bool Verify(string password, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return BCrypt.Net.BCrypt.EnhancedVerify(
            password,
            passwordHash,
            HashType.SHA384);
    }
}

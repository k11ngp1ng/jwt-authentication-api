using JwtAuthenticationApi.Application.DTOs.Auth;
using JwtAuthenticationApi.Application.Exceptions;
using JwtAuthenticationApi.Application.Interfaces;
using JwtAuthenticationApi.Application.Services;
using JwtAuthenticationApi.Domain.Entities;

namespace JwtAuthenticationApi.Tests.Unit;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldNormalizeAndPersistUserWithHashedPassword()
    {
        var repository = new InMemoryUserRepository();
        var service = new AuthService(
            repository,
            new FakePasswordHasher(),
            new FakeJwtTokenService());
        var request = new RegisterRequest
        {
            Username = "  new_user  ",
            Email = "  USER@Example.com  ",
            Password = "StrongPassword@123"
        };

        var response = await service.RegisterAsync(request);
        var user = Assert.Single(repository.Users);

        Assert.Equal("new_user", user.Username);
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("hashed::StrongPassword@123", user.PasswordHash);
        Assert.Equal("test-token", response.AccessToken);
    }

    [Fact]
    public async Task LoginAsync_ShouldRejectInvalidCredentials()
    {
        var service = new AuthService(
            new InMemoryUserRepository(),
            new FakePasswordHasher(),
            new FakeJwtTokenService());
        var request = new LoginRequest
        {
            Email = "missing@example.com",
            Password = "StrongPassword@123"
        };

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => service.LoginAsync(request));
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        public List<User> Users { get; } = [];

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Users.SingleOrDefault(user => user.Email == email));
        }

        public Task<bool> EmailExistsAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Users.Any(user => user.Email == email));
        }

        public Task<bool> UsernameExistsAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Users.Any(user => user.Username == username));
        }

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            Users.Add(user);
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed::{password}";

        public bool Verify(string password, string passwordHash) =>
            passwordHash == Hash(password);
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public AuthResponse GenerateToken(User user) =>
            new("test-token", "Bearer", DateTimeOffset.UtcNow.AddMinutes(30));
    }
}

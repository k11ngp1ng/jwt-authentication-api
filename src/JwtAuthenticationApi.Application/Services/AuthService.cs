using JwtAuthenticationApi.Application.DTOs.Auth;
using JwtAuthenticationApi.Application.Exceptions;
using JwtAuthenticationApi.Application.Interfaces;
using JwtAuthenticationApi.Domain.Entities;

namespace JwtAuthenticationApi.Application.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = request.Email.Trim().ToLowerInvariant();
        var username = request.Username.Trim();

        if (await userRepository.EmailExistsAsync(email, cancellationToken))
        {
            throw new ResourceConflictException("Email is already registered.");
        }

        if (await userRepository.UsernameExistsAsync(username, cancellationToken))
        {
            throw new ResourceConflictException("Username is already in use.");
        }

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = new User(username, email, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);

        return jwtTokenService.GenerateToken(user);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        return jwtTokenService.GenerateToken(user);
    }
}

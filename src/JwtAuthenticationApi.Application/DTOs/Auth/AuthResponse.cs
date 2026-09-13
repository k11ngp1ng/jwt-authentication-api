namespace JwtAuthenticationApi.Application.DTOs.Auth;

public sealed record AuthResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAtUtc);

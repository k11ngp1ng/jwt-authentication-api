using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtAuthenticationApi.Application.DTOs.Auth;
using JwtAuthenticationApi.Application.Interfaces;
using JwtAuthenticationApi.Application.Services;
using JwtAuthenticationApi.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthenticationApi.Infrastructure.Authentication;

public sealed class JwtTokenService(IOptions<JwtSettings> jwtOptions) : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public AuthResponse GenerateToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var secretKey = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        if (secretKey.Length < 32)
        {
            throw new InvalidOperationException("JWT secret key must contain at least 32 bytes.");
        }

        var issuedAt = DateTimeOffset.UtcNow;
        var expiresAt = issuedAt.AddMinutes(_jwtSettings.ExpirationMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtClaimNames.UserId, user.Id.ToString()),
            new Claim(JwtClaimNames.Email, user.Email),
            new Claim(JwtClaimNames.Role, user.Role.ToString()),
            new Claim(JwtClaimNames.Username, user.Username)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(secretKey),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: issuedAt.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            "Bearer",
            expiresAt);
    }
}

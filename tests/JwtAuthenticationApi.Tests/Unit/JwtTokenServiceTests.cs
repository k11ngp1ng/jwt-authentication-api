using System.IdentityModel.Tokens.Jwt;
using JwtAuthenticationApi.Application.Services;
using JwtAuthenticationApi.Domain.Entities;
using JwtAuthenticationApi.Domain.Enums;
using JwtAuthenticationApi.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

namespace JwtAuthenticationApi.Tests.Unit;

public sealed class JwtTokenServiceTests
{
    [Fact]
    public void GenerateToken_ShouldIncludeRequiredClaims()
    {
        var settings = Options.Create(new JwtSettings
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SecretKey = new string('s', 64),
            ExpirationMinutes = 30
        });
        var user = new User("admin_user", "ADMIN@example.com", "password-hash", UserRole.Admin);
        var service = new JwtTokenService(settings);

        var response = service.GenerateToken(user);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response.AccessToken);

        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal("test-issuer", token.Issuer);
        Assert.Contains("test-audience", token.Audiences);
        Assert.Equal(user.Id.ToString(), token.Claims.Single(c => c.Type == JwtClaimNames.UserId).Value);
        Assert.Equal("admin@example.com", token.Claims.Single(c => c.Type == JwtClaimNames.Email).Value);
        Assert.Equal("Admin", token.Claims.Single(c => c.Type == JwtClaimNames.Role).Value);
        Assert.True(response.ExpiresAtUtc > DateTimeOffset.UtcNow);
    }
}

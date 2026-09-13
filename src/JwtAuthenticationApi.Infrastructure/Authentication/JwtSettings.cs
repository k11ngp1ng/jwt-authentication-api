using System.ComponentModel.DataAnnotations;

namespace JwtAuthenticationApi.Infrastructure.Authentication;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    [MinLength(32)]
    public string SecretKey { get; init; } = string.Empty;

    [Range(1, 1_440)]
    public int ExpirationMinutes { get; init; } = 60;
}

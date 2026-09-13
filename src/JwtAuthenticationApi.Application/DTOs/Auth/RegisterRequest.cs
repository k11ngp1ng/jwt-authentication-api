using System.ComponentModel.DataAnnotations;

namespace JwtAuthenticationApi.Application.DTOs.Auth;

public sealed class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(
        @"^[a-zA-Z0-9_]+$",
        ErrorMessage = "Username may contain only letters, numbers, and underscores.")]
    public string Username { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).+$",
        ErrorMessage = "Password must contain uppercase, lowercase, number, and special character.")]
    public string Password { get; init; } = string.Empty;
}

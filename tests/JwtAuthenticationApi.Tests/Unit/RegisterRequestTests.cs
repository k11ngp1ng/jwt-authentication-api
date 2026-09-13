using System.ComponentModel.DataAnnotations;
using JwtAuthenticationApi.Application.DTOs.Auth;

namespace JwtAuthenticationApi.Tests.Unit;

public sealed class RegisterRequestTests
{
    [Fact]
    public void Validation_ShouldRejectWeakPassword()
    {
        var request = new RegisterRequest
        {
            Username = "valid_user",
            Email = "user@example.com",
            Password = "password"
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(
            validationResults,
            result => result.MemberNames.Contains(nameof(RegisterRequest.Password)));
    }
}

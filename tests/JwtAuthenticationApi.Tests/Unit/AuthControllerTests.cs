using JwtAuthenticationApi.Api.Controllers;
using JwtAuthenticationApi.Application.DTOs.Auth;
using JwtAuthenticationApi.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthenticationApi.Tests.Unit;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task Register_ShouldReturnCreatedWithToken()
    {
        var expectedResponse = CreateAuthResponse();
        var controller = new AuthController(new FakeAuthService(expectedResponse));
        var request = new RegisterRequest
        {
            Username = "new_user",
            Email = "user@example.com",
            Password = "StrongPassword@123"
        };

        var result = await controller.Register(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        Assert.Same(expectedResponse, objectResult.Value);
    }

    [Fact]
    public async Task Login_ShouldReturnOkWithToken()
    {
        var expectedResponse = CreateAuthResponse();
        var controller = new AuthController(new FakeAuthService(expectedResponse));
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "StrongPassword@123"
        };

        var result = await controller.Login(request, CancellationToken.None);

        var objectResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expectedResponse, objectResult.Value);
    }

    private static AuthResponse CreateAuthResponse() =>
        new("test-token", "Bearer", DateTimeOffset.UtcNow.AddMinutes(30));

    private sealed class FakeAuthService(AuthResponse response) : IAuthService
    {
        public Task<AuthResponse> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(response);
        }

        public Task<AuthResponse> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(response);
        }
    }
}

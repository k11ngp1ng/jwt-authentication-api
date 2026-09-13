using System.Reflection;
using System.Security.Claims;
using JwtAuthenticationApi.Api.Controllers;
using JwtAuthenticationApi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthenticationApi.Tests.Unit;

public sealed class TestControllerTests
{
    [Fact]
    public void Controller_ShouldRequireAuthentication()
    {
        var authorizeAttribute = typeof(TestController)
            .GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorizeAttribute);
    }

    [Fact]
    public void GetAdmin_ShouldRequireAdminRole()
    {
        var authorizeAttribute = typeof(TestController)
            .GetMethod(nameof(TestController.GetAdmin))!
            .GetCustomAttribute<AuthorizeAttribute>();

        Assert.Equal("Admin", authorizeAttribute?.Roles);
    }

    [Fact]
    public void GetUser_ShouldReturnAuthenticatedUserClaims()
    {
        var claims = new[]
        {
            new Claim(JwtClaimNames.UserId, Guid.NewGuid().ToString()),
            new Claim(JwtClaimNames.Email, "user@example.com"),
            new Claim(JwtClaimNames.Role, "User")
        };
        var controller = new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
                }
            }
        };

        var result = controller.GetUser();

        Assert.IsType<OkObjectResult>(result);
    }
}

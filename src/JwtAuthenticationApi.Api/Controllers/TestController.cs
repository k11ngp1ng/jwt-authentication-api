using System.Security.Claims;
using JwtAuthenticationApi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthenticationApi.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/protected")]
public sealed class TestController : ControllerBase
{
    [HttpGet("user")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetUser()
    {
        return Ok(new
        {
            Message = "You have access to the authenticated user endpoint.",
            UserId = User.FindFirstValue(JwtClaimNames.UserId),
            Email = User.FindFirstValue(JwtClaimNames.Email),
            Role = User.FindFirstValue(JwtClaimNames.Role)
        });
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetAdmin()
    {
        return Ok(new
        {
            Message = "You have access to the administrator endpoint.",
            UserId = User.FindFirstValue(JwtClaimNames.UserId),
            Role = User.FindFirstValue(JwtClaimNames.Role)
        });
    }
}

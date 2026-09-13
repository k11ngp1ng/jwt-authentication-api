using JwtAuthenticationApi.Application.DTOs.Auth;
using JwtAuthenticationApi.Domain.Entities;

namespace JwtAuthenticationApi.Application.Interfaces;

public interface IJwtTokenService
{
    AuthResponse GenerateToken(User user);
}

using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Domain.Entities;
using System.Security.Claims;

namespace TaskFlow.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    AuthResponseDto Generate(User user, string roleName, string refreshToken, DateTime refreshTokenExpiresAtUtc);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

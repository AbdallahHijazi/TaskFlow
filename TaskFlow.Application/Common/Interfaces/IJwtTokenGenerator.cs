using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    AuthResponseDto Generate(User user, string roleName);
}

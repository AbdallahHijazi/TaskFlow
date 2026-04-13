using MediatR;
using TaskFlow.Application.DTOs.Auth;

namespace TaskFlow.Application.Features.Auth.Commands;

public class RefreshTokenCommand : IRequest<AuthResponseDto>
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

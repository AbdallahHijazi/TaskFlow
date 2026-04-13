using MediatR;
using TaskFlow.Application.DTOs.Auth;

namespace TaskFlow.Application.Features.Auth.Commands;

public class LoginCommand : IRequest<AuthResponseDto>
{
    public LoginRequestDto Dto { get; set; }

    public LoginCommand(LoginRequestDto dto)
    {
        Dto = dto;
    }
}

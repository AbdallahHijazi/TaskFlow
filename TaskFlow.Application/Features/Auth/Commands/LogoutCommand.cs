using MediatR;

namespace TaskFlow.Application.Features.Auth.Commands;

public class LogoutCommand : IRequest<bool>
{
    public string RefreshToken { get; set; } = string.Empty;
}

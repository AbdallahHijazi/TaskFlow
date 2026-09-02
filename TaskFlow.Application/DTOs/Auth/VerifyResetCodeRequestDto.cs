namespace TaskFlow.Application.DTOs.Auth;

public sealed class VerifyResetCodeRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

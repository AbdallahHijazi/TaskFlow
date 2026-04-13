using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Features.Auth.Commands;

namespace TaskFlow.API.Controllers.Auth;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات تسجيل الدخول مطلوبة" });

        var result = await _mediator.Send(new LoginCommand(dto));
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "بيانات تحديث الجلسة مطلوبة" });

        var result = await _mediator.Send(new RefreshTokenCommand
        {
            AccessToken = dto.AccessToken,
            RefreshToken = dto.RefreshToken
        });

        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "رمز التحديث مطلوب" });

        await _mediator.Send(new LogoutCommand
        {
            RefreshToken = dto.RefreshToken
        });

        return NoContent();
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Features.Auth.Commands;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Notifications;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.API.Controllers.Auth;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly AppDbContext _db;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly SmtpOptions _smtp;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, AppDbContext db, IUserPasswordHasher passwordHasher,
        IOptions<SmtpOptions> smtp, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _db = db;
        _passwordHasher = passwordHasher;
        _smtp = smtp.Value;
        _logger = logger;
    }

    [AllowAnonymous]
    [EnableRateLimiting("password-reset")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto, CancellationToken cancellationToken)
    {
        const string responseMessage = "If an account exists for this email, a password reset link has been sent.";
        var email = dto?.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email)) return Ok(new { message = responseMessage });

        var user = await _db.Users.FirstOrDefaultAsync(item => item.Email != null && item.Email.ToLower() == email, cancellationToken);
        if (user == null) return Ok(new { message = responseMessage });

        var now = DateTime.UtcNow;
        var previousTokens = await _db.PasswordResetTokens
            .Where(item => item.UserId == user.Id && item.UsedAtUtc == null)
            .ToListAsync(cancellationToken);
        if (previousTokens.Any(item => item.CreatedAtUtc > now.AddMinutes(-1)))
            return Ok(new { message = responseMessage });
        foreach (var item in previousTokens) item.UsedAtUtc = now;

        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken(rawToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(30)
        });
        await _db.SaveChangesAsync(cancellationToken);

        if (!_smtp.Enabled || string.IsNullOrWhiteSpace(_smtp.Username) || string.IsNullOrWhiteSpace(_smtp.Password))
        {
            _logger.LogError("Password reset email was not sent because SMTP is disabled or its credentials are incomplete.");
        }
        else if (!string.IsNullOrWhiteSpace(user.Email))
        {
            try
            {
                var resetUrl = $"{_smtp.AppUrl.TrimEnd('/')}/auth/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(rawToken)}";
                using var client = new SmtpClient(_smtp.Host, _smtp.Port)
                {
                    EnableSsl = _smtp.UseSsl,
                    Credentials = new NetworkCredential(_smtp.Username, _smtp.Password)
                };
                using var mail = new MailMessage
                {
                    From = new MailAddress(_smtp.Username, _smtp.FromName),
                    Subject = "Reset your ORQIST password",
                    Body = $"Hello {user.Name},\n\nUse this secure link to reset your ORQIST password:\n{resetUrl}\n\nThis link expires in 30 minutes and can only be used once. If you did not request this, you can ignore this email.",
                    IsBodyHtml = false
                };
                mail.To.Add(user.Email);
                await client.SendMailAsync(mail, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Password reset email could not be sent to user {UserId}", user.Id);
            }
        }

        return Ok(new { message = responseMessage });
    }

    [AllowAnonymous]
    [EnableRateLimiting("password-reset")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto, CancellationToken cancellationToken)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Token) ||
            string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8)
            return BadRequest(new { message = "A valid reset link and a password of at least 8 characters are required." });

        var email = dto.Email.Trim().ToLowerInvariant();
        var tokenHash = HashToken(dto.Token.Trim());
        var now = DateTime.UtcNow;
        var resetToken = await _db.PasswordResetTokens.Include(item => item.User)
            .FirstOrDefaultAsync(item => item.TokenHash == tokenHash && item.User.Email != null &&
                item.User.Email.ToLower() == email, cancellationToken);

        if (resetToken == null || resetToken.UsedAtUtc != null || resetToken.ExpiresAtUtc <= now)
            return BadRequest(new { message = "This password reset link is invalid or has expired." });

        resetToken.User.Password = _passwordHasher.HashPassword(dto.NewPassword);
        resetToken.UsedAtUtc = now;
        var refreshTokens = await _db.RefreshTokens.Where(item => item.UserId == resetToken.UserId && !item.IsRevoked).ToListAsync(cancellationToken);
        foreach (var item in refreshTokens) { item.IsRevoked = true; item.RevokedAtUtc = now; }
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Your password has been reset successfully. You can now sign in." });
    }

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));

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

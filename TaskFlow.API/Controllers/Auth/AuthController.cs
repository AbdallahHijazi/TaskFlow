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
    private readonly string _resetCodeSecret;

    public AuthController(IMediator mediator, AppDbContext db, IUserPasswordHasher passwordHasher,
        IOptions<SmtpOptions> smtp, ILogger<AuthController> logger, IConfiguration configuration)
    {
        _mediator = mediator;
        _db = db;
        _passwordHasher = passwordHasher;
        _smtp = smtp.Value;
        _logger = logger;
        _resetCodeSecret = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT secret is required.");
    }

    [AllowAnonymous]
    [EnableRateLimiting("password-reset")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto, CancellationToken cancellationToken)
    {
        const string responseMessage = "If an account exists for this email, a 6-digit verification code has been sent.";
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

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashResetCode(email, code),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(10)
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
                using var client = new SmtpClient(_smtp.Host, _smtp.Port)
                {
                    EnableSsl = _smtp.UseSsl,
                    Credentials = new NetworkCredential(_smtp.Username, _smtp.Password)
                };
                using var mail = new MailMessage
                {
                    From = new MailAddress(_smtp.Username, _smtp.FromName),
                    Subject = $"{code} is your ORQIST verification code",
                    Body = BuildResetCodeEmail(user.Name, code),
                    IsBodyHtml = true
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
    [HttpPost("verify-reset-code")]
    public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeRequestDto dto, CancellationToken cancellationToken)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { message = "Enter the 6-digit code sent to your email." });

        var email = dto.Email.Trim().ToLowerInvariant();
        var code = dto.Code.Trim();
        if (code.Length != 6 || !code.All(char.IsDigit))
            return BadRequest(new { message = "Enter the 6-digit code sent to your email." });

        var now = DateTime.UtcNow;
        var user = await _db.Users.FirstOrDefaultAsync(item => item.Email != null && item.Email.ToLower() == email, cancellationToken);
        var resetCode = user == null ? null : await _db.PasswordResetTokens
            .Where(item => item.UserId == user.Id && item.UsedAtUtc == null)
            .OrderByDescending(item => item.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);

        if (resetCode == null || resetCode.ExpiresAtUtc <= now || resetCode.FailedAttempts >= 5)
            return BadRequest(new { message = "This verification code is invalid or has expired. Request a new code." });

        var submittedHash = HashResetCode(email, code);
        if (!CryptographicOperations.FixedTimeEquals(Convert.FromHexString(resetCode.TokenHash), Convert.FromHexString(submittedHash)))
        {
            resetCode.FailedAttempts++;
            await _db.SaveChangesAsync(cancellationToken);
            return BadRequest(new { message = "The verification code is incorrect." });
        }

        return Ok(new { resetToken = CreateResetGrant(resetCode.Id, email, now.AddMinutes(10)) });
    }

    [AllowAnonymous]
    [EnableRateLimiting("password-reset")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto, CancellationToken cancellationToken)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.ResetToken) ||
            string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8)
            return BadRequest(new { message = "A verified reset request and a password of at least 8 characters are required." });

        var email = dto.Email.Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;
        var resetCodeId = ValidateResetGrant(dto.ResetToken, email, now);
        if (resetCodeId == null)
            return BadRequest(new { message = "Your verification session is invalid or has expired. Request a new code." });

        var user = await _db.Users.FirstOrDefaultAsync(item => item.Email != null && item.Email.ToLower() == email, cancellationToken);
        var resetToken = user == null ? null : await _db.PasswordResetTokens
            .FirstOrDefaultAsync(item => item.Id == resetCodeId && item.UserId == user.Id && item.UsedAtUtc == null, cancellationToken);

        if (resetToken == null || resetToken.ExpiresAtUtc <= now)
            return BadRequest(new { message = "Your verification session is invalid or has expired. Request a new code." });

        user!.Password = _passwordHasher.HashPassword(dto.NewPassword);
        resetToken.UsedAtUtc = now;
        var refreshTokens = await _db.RefreshTokens.Where(item => item.UserId == resetToken.UserId && !item.IsRevoked).ToListAsync(cancellationToken);
        foreach (var item in refreshTokens) { item.IsRevoked = true; item.RevokedAtUtc = now; }
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Your password has been reset successfully. You can now sign in." });
    }

    private string HashResetCode(string email, string code) => Convert.ToHexString(HMACSHA256.HashData(
        System.Text.Encoding.UTF8.GetBytes(_resetCodeSecret), System.Text.Encoding.UTF8.GetBytes($"{email}:{code}")));

    private string CreateResetGrant(Guid resetCodeId, string email, DateTime expiresAtUtc)
    {
        var payload = $"{resetCodeId}:{new DateTimeOffset(expiresAtUtc).ToUnixTimeSeconds()}:{email}";
        var encodedPayload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
        var signature = Convert.ToBase64String(HMACSHA256.HashData(System.Text.Encoding.UTF8.GetBytes(_resetCodeSecret), System.Text.Encoding.UTF8.GetBytes(encodedPayload)));
        return $"{WebUtility.UrlEncode(encodedPayload)}.{WebUtility.UrlEncode(signature)}";
    }

    private Guid? ValidateResetGrant(string grant, string email, DateTime now)
    {
        var parts = grant.Split('.', 2);
        if (parts.Length != 2) return null;
        try
        {
            var encodedPayload = WebUtility.UrlDecode(parts[0]);
            var suppliedSignature = Convert.FromBase64String(WebUtility.UrlDecode(parts[1]));
            var expectedSignature = HMACSHA256.HashData(System.Text.Encoding.UTF8.GetBytes(_resetCodeSecret), System.Text.Encoding.UTF8.GetBytes(encodedPayload));
            if (!CryptographicOperations.FixedTimeEquals(suppliedSignature, expectedSignature)) return null;
            var payload = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedPayload)).Split(':', 3);
            if (payload.Length != 3 || !Guid.TryParse(payload[0], out var id) || !long.TryParse(payload[1], out var expiry) || payload[2] != email) return null;
            return DateTimeOffset.FromUnixTimeSeconds(expiry).UtcDateTime > now ? id : null;
        }
        catch (FormatException) { return null; }
    }

    private static string BuildResetCodeEmail(string? name, string code)
    {
        var safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(name) ? "there" : name);
        return $$"""
        <!doctype html><html><body style="margin:0;background:#f5f7fb;font-family:Arial,sans-serif;color:#172033">
        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f5f7fb;padding:32px 16px"><tr><td align="center">
        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:560px;background:#fff;border:1px solid #e5e9f2;border-radius:20px;overflow:hidden">
        <tr><td style="padding:26px 32px;background:linear-gradient(135deg,#4f46e5,#7c3aed);color:#fff"><div style="font-size:24px;font-weight:800;letter-spacing:.08em">ORQIST</div><div style="margin-top:5px;font-size:12px;opacity:.82">WORK EXECUTION PLATFORM</div></td></tr>
        <tr><td style="padding:34px 32px"><h1 style="margin:0 0 12px;font-size:23px">Reset your password</h1><p style="margin:0 0 22px;color:#64748b;line-height:1.65">Hello {{safeName}}, use this verification code to continue resetting your ORQIST password.</p>
        <div style="padding:20px;border:1px solid #ddd9fe;border-radius:16px;background:#f7f5ff;text-align:center"><div style="font-size:11px;font-weight:700;color:#6d4ce8;letter-spacing:.12em">VERIFICATION CODE</div><div style="margin-top:8px;font-size:36px;font-weight:800;letter-spacing:.22em;color:#30236f">{{code}}</div></div>
        <p style="margin:22px 0 0;color:#64748b;font-size:13px;line-height:1.6">This code expires in <strong>10 minutes</strong> and can be used once. Never share it with anyone.</p>
        <p style="margin:14px 0 0;color:#94a3b8;font-size:12px">If you did not request a password reset, you can safely ignore this email.</p></td></tr>
        <tr><td style="padding:18px 32px;border-top:1px solid #edf0f5;color:#94a3b8;font-size:11px">Sent securely by ORQIST Notifications</td></tr></table>
        </td></tr></table></body></html>
        """;
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

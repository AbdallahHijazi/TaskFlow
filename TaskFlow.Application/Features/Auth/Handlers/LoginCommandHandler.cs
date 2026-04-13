using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Features.Auth.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Auth.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IRepository<User> _usersRepository;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IAuthSettingsProvider _authSettingsProvider;

    public LoginCommandHandler(
        IRepository<User> usersRepository,
        IRepository<RefreshToken> refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IUserPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IAuthSettingsProvider authSettingsProvider)
    {
        _usersRepository = usersRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _authSettingsProvider = authSettingsProvider;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Dto.Email) || string.IsNullOrWhiteSpace(request.Dto.Password))
                throw new UnauthorizedException("البريد الإلكتروني أو كلمة المرور غير صحيحة");

            var normalizedEmail = request.Dto.Email.Trim().ToLowerInvariant();

            var user = await _usersRepository
                .GetAll()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail, cancellationToken);

            if (user == null || string.IsNullOrWhiteSpace(user.Password))
                throw new UnauthorizedException("البريد الإلكتروني أو كلمة المرور غير صحيحة");

            var passwordValid = _passwordHasher.VerifyPassword(user.Password, request.Dto.Password);
            if (!passwordValid)
                throw new UnauthorizedException("البريد الإلكتروني أو كلمة المرور غير صحيحة");

            var refreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(_authSettingsProvider.RefreshTokenExpiryDays);
            var refreshToken = new RefreshToken
            {
                Token = refreshTokenString,
                UserId = user.Id,
                ExpiresAtUtc = refreshTokenExpiresAtUtc,
                CreatedAtUtc = DateTime.UtcNow,
                IsRevoked = false
            };

            _refreshTokenRepository.Add(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var roleName = user.Role?.RoleName ?? "User";
            return _jwtTokenGenerator.Generate(user, roleName, refreshTokenString, refreshTokenExpiresAtUtc);
        }
        catch (UnauthorizedException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new UnauthorizedException("حدث خطأ أثناء تسجيل الدخول");
        }
    }
}

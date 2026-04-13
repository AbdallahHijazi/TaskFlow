using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Features.Auth.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Auth.Handlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IAuthSettingsProvider _authSettingsProvider;

    public RefreshTokenCommandHandler(
        IRepository<RefreshToken> refreshTokenRepository,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator,
        IAuthSettingsProvider authSettingsProvider)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
        _authSettingsProvider = authSettingsProvider;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var principal = _jwtTokenGenerator.GetPrincipalFromExpiredToken(request.AccessToken);
            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedException("رمز التحديث غير صالح أو منتهي الصلاحية");

            var storedRefreshToken = await _refreshTokenRepository
                .GetAll()
                .FirstOrDefaultAsync(rt =>
                    rt.Token == request.RefreshToken &&
                    rt.UserId == userId &&
                    !rt.IsRevoked &&
                    rt.ExpiresAtUtc > DateTime.UtcNow, cancellationToken);

            if (storedRefreshToken == null)
                throw new UnauthorizedException("رمز التحديث غير صالح أو منتهي الصلاحية");

            storedRefreshToken.IsRevoked = true;
            storedRefreshToken.RevokedAtUtc = DateTime.UtcNow;
            _refreshTokenRepository.Update(storedRefreshToken);

            var newRefreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var newRefreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(_authSettingsProvider.RefreshTokenExpiryDays);
            var newRefreshToken = new RefreshToken
            {
                Token = newRefreshTokenString,
                UserId = userId,
                ExpiresAtUtc = newRefreshTokenExpiresAtUtc,
                CreatedAtUtc = DateTime.UtcNow,
                IsRevoked = false
            };

            _refreshTokenRepository.Add(newRefreshToken);

            var user = await _userRepository
                .GetAll()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                throw new UnauthorizedException("رمز التحديث غير صالح أو منتهي الصلاحية");

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var roleName = user.Role?.RoleName ?? "User";
            return _jwtTokenGenerator.Generate(user, roleName, newRefreshTokenString, newRefreshTokenExpiresAtUtc);
        }
        catch (UnauthorizedException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new UnauthorizedException("رمز التحديث غير صالح أو منتهي الصلاحية");
        }
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Features.Auth.Commands;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Auth.Handlers;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IRepository<RefreshToken> refreshTokenRepository, IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = await _refreshTokenRepository
                .GetAll()
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && !rt.IsRevoked, cancellationToken);

            if (refreshToken == null)
                return true;

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAtUtc = DateTime.UtcNow;
            _refreshTokenRepository.Update(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception)
        {
            return true;
        }
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.Features.Auth.Commands;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Features.Auth.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IRepository<User> _usersRepository;
    private readonly IUserPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IRepository<User> usersRepository,
        IUserPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
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

        var roleName = user.Role?.RoleName ?? "User";
        return _jwtTokenGenerator.Generate(user, roleName);
    }
}

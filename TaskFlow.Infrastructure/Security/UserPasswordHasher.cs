using Microsoft.AspNetCore.Identity;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Infrastructure.Security;

internal sealed class CredentialUser;

public sealed class UserPasswordHasher : IUserPasswordHasher
{
    private readonly PasswordHasher<CredentialUser> _hasher = new();
    private readonly CredentialUser _marker = new();

    public string HashPassword(string password) => _hasher.HashPassword(_marker, password);

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword))
            return false;

        return _hasher.VerifyHashedPassword(_marker, hashedPassword, providedPassword)
               == PasswordVerificationResult.Success;
    }
}

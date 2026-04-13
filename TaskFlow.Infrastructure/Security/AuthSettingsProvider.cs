using Microsoft.Extensions.Options;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Infrastructure.Security;

public class AuthSettingsProvider : IAuthSettingsProvider
{
    private readonly JwtSettings _jwtSettings;

    public AuthSettingsProvider(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public int RefreshTokenExpiryDays => _jwtSettings.RefreshTokenExpiryDays;
}

using System.Net.Mail;

namespace TaskFlow.Application.Common.Security;

public static class EmailAddressPolicy
{
    public static string NormalizeAndValidate(string? value)
    {
        var email = value?.Trim().ToLowerInvariant() ?? string.Empty;
        if (email.Length is < 5 or > 254 || !MailAddress.TryCreate(email, out var parsed) ||
            !string.Equals(parsed.Address, email, StringComparison.OrdinalIgnoreCase) ||
            !email.Contains('.') || email.EndsWith('.'))
            throw new InvalidOperationException("Enter a valid email address.");
        return email;
    }
}

using Microsoft.AspNetCore.Mvc;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.API.Infrastructure;

internal static class ApiErrors
{
    public static (int StatusCode, ApiErrorResponse Body) Map(Exception ex, string traceId) => ex switch
    {
        ValidationException v => (StatusCodes.Status400BadRequest, Build(v.Message, "400", traceId, v.Errors)),
        NotFoundException n => (StatusCodes.Status404NotFound, Build(n.Message, "404", traceId)),
        BadRequestException b => (StatusCodes.Status400BadRequest, Build(b.Message, "400", traceId)),
        UnauthorizedException u => (StatusCodes.Status401Unauthorized, Build(u.Message, "401", traceId)),
        StatusAlreadyExistsException s => (StatusCodes.Status409Conflict, Build(s.Message, "409", traceId)),
        InvalidOperationException i => (StatusCodes.Status400BadRequest, Build(i.Message, "400", traceId)),
        _ => (StatusCodes.Status500InternalServerError, Build("An unexpected server error occurred.", "500", traceId))
    };

    public static IActionResult From(Exception ex)
    {
        var (statusCode, body) = Map(ex, string.Empty);
        return new ObjectResult(body) { StatusCode = statusCode };
    }

    private static ApiErrorResponse Build(
        string message,
        string errorCode,
        string traceId,
        IReadOnlyDictionary<string, string[]>? validationErrors = null)
    {
        return new ApiErrorResponse
        {
            Message = EnsureEnglish(message, errorCode),
            ErrorCode = errorCode,
            ValidationErrors = validationErrors is null ? null : validationErrors.ToDictionary(
                item => item.Key,
                item => item.Value.Select(value => EnsureEnglish(value, errorCode,
                    $"Please provide a valid value for {Humanize(item.Key)}.")).Distinct().ToArray()),
            TraceId = traceId
        };
    }

    private static string EnsureEnglish(string? message, string errorCode, string? fallback = null)
    {
        var value = message?.Trim() ?? string.Empty;
        var invalid = string.IsNullOrWhiteSpace(value) || value.Any(character => character is >= '\u0600' and <= '\u06ff') ||
            value.Contains('Ø') || value.Contains('Ù') || value.Contains('Ã') || value.Contains('Â');
        if (!invalid) return value;
        if (!string.IsNullOrWhiteSpace(fallback)) return fallback;
        return errorCode switch
        {
            "400" => "Please review the entered information and try again.",
            "401" => "Your session has expired. Please sign in again.",
            "403" => "You do not have permission to perform this action.",
            "404" => "The requested resource could not be found.",
            "409" => "This action conflicts with existing data.",
            _ => "An unexpected server error occurred. Please try again shortly."
        };
    }

    private static string Humanize(string value) => string.Concat(value.Select((character, index) =>
        index > 0 && char.IsUpper(character) ? $" {char.ToLowerInvariant(character)}" : char.ToLowerInvariant(character).ToString()));
}

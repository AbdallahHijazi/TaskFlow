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
        _ => (StatusCodes.Status500InternalServerError, Build("حدث خطأ غير متوقع.", "500", traceId))
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
            Message = message,
            ErrorCode = errorCode,
            ValidationErrors = validationErrors is null ? null : new Dictionary<string, string[]>(validationErrors),
            TraceId = traceId
        };
    }
}

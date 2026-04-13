using Microsoft.AspNetCore.Mvc;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.API.Infrastructure;

internal static class ApiErrors
{
    public static (int StatusCode, object Body) Map(Exception ex) => ex switch
    {
        NotFoundException n => (StatusCodes.Status404NotFound, new { Message = n.Message }),
        BadRequestException b => (StatusCodes.Status400BadRequest, new { Message = b.Message }),
        UnauthorizedException u => (StatusCodes.Status401Unauthorized, new { Message = u.Message }),
        StatusAlreadyExistsException s => (StatusCodes.Status409Conflict, new { Message = s.Message }),
        InvalidOperationException i => (StatusCodes.Status400BadRequest, new { Message = i.Message }),
        _ => (StatusCodes.Status500InternalServerError, new { Message = "حدث خطأ غير متوقع." })
    };

    public static IActionResult From(Exception ex)
    {
        var (statusCode, body) = Map(ex);
        return new ObjectResult(body) { StatusCode = statusCode };
    }
}

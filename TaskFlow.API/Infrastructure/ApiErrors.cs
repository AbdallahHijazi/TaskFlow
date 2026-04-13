using Microsoft.AspNetCore.Mvc;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.API.Infrastructure
{
    internal static class ApiErrors
    {
        public static IActionResult From(Exception ex)
        {
            return ex switch
            {
                NotFoundException n => new NotFoundObjectResult(new { Message = n.Message }),
                BadRequestException b => new BadRequestObjectResult(new { Message = b.Message }),
                StatusAlreadyExistsException s => new ConflictObjectResult(new { Message = s.Message }),
                InvalidOperationException i => new ObjectResult(new { Message = i.Message })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                },
                _ => new ObjectResult(new { Message = "حدث خطأ غير متوقع." })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}

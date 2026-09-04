using System.Text.Json;

namespace TaskFlow.API.Infrastructure;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException ex) when (context.RequestAborted.IsCancellationRequested)
        {
            // The caller has gone away, so there is no response left to write. Do not
            // propagate the exception to the developer exception page.
            _logger.LogInformation(ex, "Request {TraceId} was cancelled by the client.", context.TraceIdentifier);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "Unhandled exception after the response started. TraceId: {TraceId}", context.TraceIdentifier);
                context.Abort();
                return;
            }

            var traceId = context.TraceIdentifier;
            var (statusCode, body) = ApiErrors.Map(ex, traceId);
            if (ex is OperationCanceledException)
                _logger.LogWarning(ex, "Upstream operation timed out. TraceId: {TraceId}", traceId);
            else
                _logger.LogError(ex, "Unhandled API exception. TraceId: {TraceId}", traceId);

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";
            await JsonSerializer.SerializeAsync(context.Response.Body, body, JsonOptions, CancellationToken.None);
        }
    }
}

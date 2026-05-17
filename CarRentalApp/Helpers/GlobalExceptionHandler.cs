using CarRentalApp.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApp.Helpers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response has already started; cannot write error response.");
                return false;
            }

            var (statusCode, title, isExpected) = MapException(exception);

            if (isExpected)
            {
                _logger.LogWarning(exception,
                    "Handled {ExceptionType} at {Method} {Endpoint} by {User} | Trace={TraceId}",
                    exception.GetType().Name,
                    context.Request.Method,
                    context.Request.Path,
                    context.User.Identity?.Name ?? "Anonymous",
                    context.TraceIdentifier);
            }
            else
            {
                _logger.LogError(exception,
                    "Unhandled exception at {Method} {Endpoint} by {User} | Trace={TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.User.Identity?.Name ?? "Anonymous",
                    context.TraceIdentifier);
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = isExpected
                    ? exception.Message
                    : "An unexpected error occurred. Please contact support.",
                Instance = context.Request.Path,
                Type = $"https://httpstatuses.io/{statusCode}"
            };

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            if (isExpected && exception is AppException appEx)
                problemDetails.Extensions["code"] = appEx.Code;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(
                problemDetails, cancellationToken);

            return true;
        }

        private static (int StatusCode, string Title, bool IsExpected) MapException(Exception ex) => ex switch
        {
            EntityAlreadyExistsException => (StatusCodes.Status409Conflict, "Resource already exists", true),
            EntityNotAuthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized", true),
            EntityForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", true),
            EntityNotFoundException => (StatusCodes.Status404NotFound, "Resource not found", true),
            InvalidArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument", true),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error", false)
        };
    }
}

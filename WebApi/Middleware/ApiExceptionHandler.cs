using ApplicationServices.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Middleware
{
    public class ApiExceptionHandler(IProblemDetailsService problemDetails, ILogger<ApiExceptionHandler> logger)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            int status = exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Unhandled exception");

            context.Response.StatusCode = status;

            return await problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = status,
                    // Never leak internal error details to the client
                    Title = status == StatusCodes.Status500InternalServerError
                        ? "An unexpected error occurred."
                        : exception.Message
                }
            });
        }
    }
}
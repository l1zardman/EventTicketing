using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.Api;

// Exceptions are now reserved for the genuinely exceptional. Expected failures
// (not found, sold out) travel as Results and are mapped at the endpoint. This
// leaves two jobs: input-validation failures (still thrown by the pipeline) and
// unexpected errors (bugs, outages) that must never leak details.
public sealed class ExceptionHandlingMiddleware
{
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
        catch (ValidationException ex)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Validation failed", ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            await WriteProblem(context, StatusCodes.Status409Conflict,
                "Concurrency conflict", "The event was modified by another request. Please retry.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblem(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred", null);
        }
    }

    private static Task WriteProblem(HttpContext ctx, int status, string title, string? detail)
    {
        ctx.Response.StatusCode = status;
        return ctx.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = title, Detail = detail });
    }
}

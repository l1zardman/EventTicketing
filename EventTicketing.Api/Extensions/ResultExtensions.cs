using EventTicketing.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace EventTicketing.Api.Extensions;

public static class ResultExtensions
{
    // Turns a failed Result into an RFC-7807 problem response, choosing the status
    // code from the error's Type. Success is never a problem — the caller checks first.
    public static IResult ToProblem(this Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("A successful result cannot be converted to a problem.");

        var status = result.Error.Type switch
        {
            ErrorType.NotFound   => StatusCodes.Status404NotFound,
            ErrorType.Conflict   => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _                    => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(statusCode: status, title: result.Error.Code, detail: result.Error.Description);
    }
}

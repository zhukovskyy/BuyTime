using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BuyTime_Api.Controllers;

[ApiController]
public abstract class ApiController : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                throw new InvalidOperationException("User ID missing or invalid in JWT claims. Ensure [Authorize] is applied to the endpoint.");
            }

            return userId;
        }
    }

    protected IActionResult Problem(List<Error> errors)
    {
        HttpContext.Items["errors"] = errors;

        var firstError = errors.FirstOrDefault();

        var statusCode = firstError.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError,
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = "Error",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Detail = "Multiple errors occurred",
        };

        problemDetails.Extensions["errors"] = errors;

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode,
        };
    }
}
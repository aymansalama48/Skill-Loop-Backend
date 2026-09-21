using Skill_Loop.Application.Common.Constants;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResponse(this Result result, HttpContext? httpContext = null)
        => Handle(result, httpContext);

    public static IResult ToHttpResponse<T>(this Result<T> result, HttpContext? httpContext = null)
        => Handle(result, httpContext);

    private static IResult Handle(Result result, HttpContext? httpContext)
    {
        if (result.Succeeded)
        {
            return string.IsNullOrWhiteSpace(result.Message)
                ? Results.NoContent()
                : Results.Ok(new { message = result.Message });
        }

        return MapError(result.Errors, httpContext);
    }

    private static IResult Handle<T>(Result<T> result, HttpContext? httpContext)
    {
        if (result.Succeeded)
        {
            return Results.Ok(new { data = result.Data });
        }

        return MapError(result.Errors, httpContext);
    }

    private static IResult MapError(IEnumerable<Error> errors, HttpContext? httpContext)
    {
        var errorList = errors?.ToList() ?? new List<Error>();
        var primaryError = errorList.FirstOrDefault();
        var errorType = primaryError?.Type ?? ErrorType.Failure;

        var (statusCode, title, defaultDetail) = errorType switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation Error", "حدث خطأ في البيانات المدخلة."),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized", "يلزم تسجيل الدخول أولاً."),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden", "ليس لديك صلاحية للوصول."),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found", "العنصر المطلوب غير موجود."),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict", "حدث تعارض في البيانات."),
            ErrorType.Unexpected => (StatusCodes.Status500InternalServerError, "Server Error", "حدث خطأ غير متوقع في الخادم."),
            _ => (StatusCodes.Status400BadRequest, "Bad Request", "فشلت العملية.")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = primaryError?.Description ?? defaultDetail,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        problemDetails.Extensions["errors"] = errorList.Select(e => new
        {
            e.Code,
            e.Description,
            Type = e.Type.ToString()
        });

        if (httpContext != null)
        {
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            if (httpContext.Items.TryGetValue(CorrelationConstants.HeaderKey, out var correlationId))
            {
                problemDetails.Extensions["correlationId"] = correlationId;
            }

            problemDetails.Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}";
        }

        return Results.Json(
            problemDetails,
            statusCode: statusCode,
            contentType: "application/problem+json");
    }
}
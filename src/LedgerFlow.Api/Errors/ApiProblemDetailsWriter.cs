using Microsoft.AspNetCore.Mvc;

namespace LedgerFlow.Api.Errors;

public static class ApiProblemDetailsWriter
{
    public static void Enrich(
        ProblemDetails details,
        HttpContext context,
        string? code = null,
        string? message = null)
    {
        details.Extensions["success"] = false;
        if (message is not null || !details.Extensions.ContainsKey("message"))
            details.Extensions["message"] = message ?? details.Detail ?? details.Title ?? "The request failed.";
        if (code is not null || !details.Extensions.ContainsKey("code"))
            details.Extensions["code"] = code ?? "request_failed";
        details.Extensions["traceId"] = context.TraceIdentifier;
    }

    public static async Task WriteAsync(
        HttpContext context,
        int status,
        string title,
        string message,
        string code)
    {
        context.Response.StatusCode = status;
        var details = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = message,
            Instance = context.Request.Path
        };
        Enrich(details, context, code, message);

        var service = context.RequestServices.GetRequiredService<IProblemDetailsService>();
        if (!await service.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = details
            }))
        {
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(details);
        }
    }

    public static Task WriteStatusCodeAsync(HttpContext context) => context.Response.StatusCode switch
    {
        StatusCodes.Status400BadRequest => WriteAsync(
            context,
            StatusCodes.Status400BadRequest,
            "Invalid request",
            "The request could not be understood or contains invalid data.",
            "bad_request"),
        StatusCodes.Status401Unauthorized => WriteAsync(
            context,
            StatusCodes.Status401Unauthorized,
            "Authentication required",
            "Authenticate with a valid access token to access this resource.",
            "authentication_required"),
        StatusCodes.Status403Forbidden => WriteAsync(
            context,
            StatusCodes.Status403Forbidden,
            "Access denied",
            "You do not have permission to perform this operation.",
            "access_denied"),
        StatusCodes.Status404NotFound => WriteAsync(
            context,
            StatusCodes.Status404NotFound,
            "Resource not found",
            "The requested endpoint or resource was not found.",
            "resource_not_found"),
        StatusCodes.Status405MethodNotAllowed => WriteAsync(
            context,
            StatusCodes.Status405MethodNotAllowed,
            "Method not allowed",
            "The HTTP method is not supported by this endpoint.",
            "method_not_allowed"),
        StatusCodes.Status429TooManyRequests => WriteAsync(
            context,
            StatusCodes.Status429TooManyRequests,
            "Too many requests",
            "The request limit was exceeded. Wait before trying again.",
            "rate_limit_exceeded"),
        _ => WriteAsync(
            context,
            context.Response.StatusCode,
            "Request failed",
            "The request could not be completed.",
            "request_failed")
    };
}

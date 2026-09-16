using System.Data.Common;
using LedgerFlow.Application.Common;
using LedgerFlow.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LedgerFlow.Api.Errors;

public sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var error = MapException(exception);

        if (error.Status >= StatusCodes.Status500InternalServerError)
            LogUnhandled(logger, exception, httpContext.TraceIdentifier);
        else
            LogHandled(logger, exception.GetType().Name, error.Status, httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = error.Status;
        var details = CreateProblemDetails(httpContext, exception, error);
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = details
        });
    }

    private static ErrorDescriptor MapException(Exception exception) => exception switch
    {
        ApplicationValidationException => new(
            StatusCodes.Status400BadRequest,
            "Request validation failed",
            "One or more fields contain invalid values.",
            "validation_failed"),
        DomainException => new(
            StatusCodes.Status422UnprocessableEntity,
            "Business rule violation",
            exception.Message,
            "business_rule_violation"),
        NotFoundException => new(
            StatusCodes.Status404NotFound,
            "Resource not found",
            exception.Message,
            "resource_not_found"),
        ConflictException => new(
            StatusCodes.Status409Conflict,
            "Resource conflict",
            exception.Message,
            "resource_conflict"),
        UnauthorizedException or UnauthorizedAccessException => new(
            StatusCodes.Status401Unauthorized,
            "Authentication failed",
            exception.Message,
            "authentication_failed"),
        SqlException { Number: 2601 or 2627 } => new(
            StatusCodes.Status409Conflict,
            "Duplicate resource",
            "A resource with the same unique information already exists.",
            "duplicate_resource"),
        SqlException or DbException => new(
            StatusCodes.Status503ServiceUnavailable,
            "Database unavailable",
            "The database is temporarily unavailable. Try again later.",
            "database_unavailable"),
        TimeoutException => new(
            StatusCodes.Status504GatewayTimeout,
            "Operation timed out",
            "The operation took too long to complete. Try again later.",
            "operation_timeout"),
        _ => new(
            StatusCodes.Status500InternalServerError,
            "Unexpected API failure",
            "The API could not complete the request. Use the trace identifier when contacting support.",
            "internal_server_error")
    };

    private static ProblemDetails CreateProblemDetails(
        HttpContext context,
        Exception exception,
        ErrorDescriptor error)
    {
        ProblemDetails details;
        if (exception is ApplicationValidationException validationException)
        {
            var validationDetails = new ValidationProblemDetails
            {
                Status = error.Status,
                Title = error.Title,
                Detail = error.Message,
                Instance = context.Request.Path
            };
            foreach (var validationError in validationException.Errors)
                validationDetails.Errors[validationError.Key] = validationError.Value;
            details = validationDetails;
        }
        else
        {
            details = new ProblemDetails
            {
                Status = error.Status,
                Title = error.Title,
                Detail = error.Message,
                Instance = context.Request.Path
            };
        }

        ApiProblemDetailsWriter.Enrich(details, context, error.Code, error.Message);
        return details;
    }

    private sealed record ErrorDescriptor(int Status, string Title, string Message, string Code);

    [LoggerMessage(1, LogLevel.Error, "Unhandled request exception. TraceId: {TraceId}")]
    private static partial void LogUnhandled(ILogger logger, Exception exception, string traceId);

    [LoggerMessage(2, LogLevel.Warning, "Handled {ExceptionType} with HTTP {StatusCode}. TraceId: {TraceId}")]
    private static partial void LogHandled(
        ILogger logger,
        string exceptionType,
        int statusCode,
        string traceId);
}

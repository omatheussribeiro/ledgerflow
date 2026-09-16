namespace LedgerFlow.Application.Dtos.Common;

/// <summary>Standard successful API response containing a message and result data.</summary>
public sealed record ApiResponseDto<T>(bool Success, string Message, T Data);

/// <summary>Standard successful API response without result data.</summary>
public sealed record ApiResponseDto(bool Success, string Message);

public static class ApiResponseFactory
{
    public static ApiResponseDto<T> Success<T>(T data, string message) => new(true, message, data);
    public static ApiResponseDto Success(string message) => new(true, message);
}

/// <summary>API health information.</summary>
public sealed record HealthStatusDto(string Status, DateTimeOffset Timestamp);

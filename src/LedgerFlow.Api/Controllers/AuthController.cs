using LedgerFlow.Application.Dtos.Auth;
using LedgerFlow.Application.Dtos.Common;
using LedgerFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LedgerFlow.Api.Controllers;

/// <summary>Creates and manages LedgerFlow authentication sessions.</summary>
[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>Registers a user and immediately issues an access/refresh token pair.</summary>
    /// <remarks>The password must have 10-128 characters with upper-case, lower-case and numeric characters.</remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<ApiResponseDto<AuthResponseDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Register(
        RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var session = await auth.RegisterAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponseFactory.Success(session, "User registered successfully."));
    }

    /// <summary>Authenticates a user with email and password.</summary>
    /// <returns>A successful response containing tokens and the user profile.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<ApiResponseDto<AuthResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Login(
        LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var session = await auth.LoginAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(session, "Authentication completed successfully."));
    }

    /// <summary>Rotates a valid refresh token and issues a new token pair.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType<ApiResponseDto<AuthResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Refresh(
        RefreshRequestDto request,
        CancellationToken cancellationToken)
    {
        var session = await auth.RefreshAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(session, "Session renewed successfully."));
    }

    /// <summary>Revokes the supplied refresh token.</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType<ApiResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponseDto>> Logout(
        RefreshRequestDto request,
        CancellationToken cancellationToken)
    {
        await auth.LogoutAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success("Session ended successfully."));
    }
}

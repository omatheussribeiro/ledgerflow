using System.ComponentModel.DataAnnotations;

namespace LedgerFlow.Application.Dtos.Auth;

/// <summary>Data required to create a LedgerFlow user.</summary>
public sealed record RegisterRequestDto(
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, MinLength(10), MaxLength(128)] string Password);

/// <summary>Credentials used to authenticate an existing user.</summary>
public sealed record LoginRequestDto(
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required] string Password);

/// <summary>Opaque refresh token used for rotation or revocation.</summary>
public sealed record RefreshRequestDto([Required] string RefreshToken);

/// <summary>Authenticated session returned after registration, login or token rotation.</summary>
public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    UserResponseDto User);

/// <summary>Safe user profile exposed to API clients.</summary>
public sealed record UserResponseDto(Guid Id, string Name, string Email, string Role);

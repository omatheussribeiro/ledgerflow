using System.Net.Mail;
using LedgerFlow.Application.Dtos.Auth;
using LedgerFlow.Application.Interfaces.Services;

namespace LedgerFlow.Application.Validators;

public sealed class RegisterRequestValidator : IRequestValidator<RegisterRequestDto>
{
    public void ValidateAndThrow(RegisterRequestDto request)
    {
        var errors = new ValidationErrors();
        var name = request.Name?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;

        errors.AddIf(name.Length is < 2 or > 120, nameof(request.Name), "Name must contain between 2 and 120 characters.");
        errors.AddIf(email.Length > 254 || !MailAddress.TryCreate(email, out _), nameof(request.Email), "A valid email address is required.");
        errors.AddIf(password.Length is < 10 or > 128, nameof(request.Password), "Password must contain between 10 and 128 characters.");
        errors.AddIf(!password.Any(char.IsLower), nameof(request.Password), "Password must contain a lower-case character.");
        errors.AddIf(!password.Any(char.IsUpper), nameof(request.Password), "Password must contain an upper-case character.");
        errors.AddIf(!password.Any(char.IsDigit), nameof(request.Password), "Password must contain a numeric character.");
        errors.ThrowIfInvalid();
    }
}

public sealed class LoginRequestValidator : IRequestValidator<LoginRequestDto>
{
    public void ValidateAndThrow(LoginRequestDto request)
    {
        var errors = new ValidationErrors();
        var email = request.Email?.Trim() ?? string.Empty;
        errors.AddIf(email.Length > 254 || !MailAddress.TryCreate(email, out _), nameof(request.Email), "A valid email address is required.");
        errors.AddIf(string.IsNullOrWhiteSpace(request.Password), nameof(request.Password), "Password is required.");
        errors.ThrowIfInvalid();
    }
}

public sealed class RefreshRequestValidator : IRequestValidator<RefreshRequestDto>
{
    public void ValidateAndThrow(RefreshRequestDto request)
    {
        var errors = new ValidationErrors();
        errors.AddIf(string.IsNullOrWhiteSpace(request.RefreshToken), nameof(request.RefreshToken), "Refresh token is required.");
        errors.ThrowIfInvalid();
    }
}

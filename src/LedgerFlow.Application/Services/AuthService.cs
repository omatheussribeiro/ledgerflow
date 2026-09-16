using LedgerFlow.Application.Common;
using LedgerFlow.Application.Dtos.Auth;
using LedgerFlow.Application.Interfaces.Repositories;
using LedgerFlow.Application.Interfaces.Services;
using LedgerFlow.Application.Mappings;
using LedgerFlow.Domain.Identity;

namespace LedgerFlow.Application.Services;

public sealed class AuthService(
    IUserRepository users,
    IPasswordService passwords,
    ITokenService tokens,
    IRequestValidator<RegisterRequestDto> registerValidator,
    IRequestValidator<LoginRequestDto> loginValidator,
    IRequestValidator<RefreshRequestDto> refreshValidator) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        registerValidator.ValidateAndThrow(request);
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.FindByEmailAsync(email, cancellationToken) is not null)
            throw new ConflictException("An account with this email already exists.");

        var user = User.Create(request.Name, email, passwords.Hash(request.Password));
        await users.AddAsync(user, cancellationToken);
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        loginValidator.ValidateAndThrow(request);
        var user = await users.FindByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !passwords.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshAsync(
        RefreshRequestDto request,
        CancellationToken cancellationToken)
    {
        refreshValidator.ValidateAndThrow(request);
        var nextRefresh = tokens.CreateRefreshToken();
        var userId = await users.RotateRefreshTokenAsync(
            tokens.HashRefreshToken(request.RefreshToken),
            tokens.HashRefreshToken(nextRefresh),
            DateTimeOffset.UtcNow.AddDays(7),
            cancellationToken);
        if (userId is null) throw new UnauthorizedException("Refresh token is invalid, expired or revoked.");

        var user = await users.FindByIdAsync(userId.Value, cancellationToken)
            ?? throw new UnauthorizedException("User no longer exists.");
        var access = tokens.CreateAccessToken(user);
        return new AuthResponseDto(access.Token, nextRefresh, access.ExpiresAt, user.ToDto());
    }

    public Task LogoutAsync(RefreshRequestDto request, CancellationToken cancellationToken)
    {
        refreshValidator.ValidateAndThrow(request);
        return users.RevokeRefreshTokenAsync(tokens.HashRefreshToken(request.RefreshToken), cancellationToken);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var access = tokens.CreateAccessToken(user);
        var refresh = tokens.CreateRefreshToken();
        await users.StoreRefreshTokenAsync(
            user.Id,
            tokens.HashRefreshToken(refresh),
            DateTimeOffset.UtcNow.AddDays(7),
            cancellationToken);
        return new AuthResponseDto(access.Token, refresh, access.ExpiresAt, user.ToDto());
    }
}

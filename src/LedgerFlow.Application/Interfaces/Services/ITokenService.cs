using LedgerFlow.Domain.Identity;

namespace LedgerFlow.Application.Interfaces.Services;

public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(User user);
    string CreateRefreshToken();
    string HashRefreshToken(string token);
}

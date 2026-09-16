using LedgerFlow.Domain.Identity;

namespace LedgerFlow.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task StoreRefreshTokenAsync(
        Guid userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken);
    Task<Guid?> RotateRefreshTokenAsync(
        string currentHash,
        string nextHash,
        DateTimeOffset nextExpiresAt,
        CancellationToken cancellationToken);
    Task RevokeRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken);
}
